using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using WorldNMilSim.Units;

namespace WorldNMilSim.Systems;

public class AiSystem : ISystem
{
    private readonly TerrainMap _terrainMap;
    private readonly Entity _defconEntity;
    private readonly Entity _diplomacyEntity;
    private readonly System.Random _random = new();

    private const int MaxAiUnits = 10;
    private static readonly string[] PlacementPriority = { "silo", "radar_station", "army", "destroyer", "submarine", "airbase", "carrier" };
    private static readonly Dictionary<string, int> MaxPerType = new()
    {
        ["silo"] = 1,
        ["radar_station"] = 1,
        ["airbase"] = 1,
        ["destroyer"] = 3,
        ["submarine"] = 2,
        ["carrier"] = 1,
        ["army"] = 2,
    };

    public AiSystem(TerrainMap terrainMap, Entity defconEntity, Entity diplomacyEntity)
    {
        _terrainMap = terrainMap;
        _defconEntity = defconEntity;
        _diplomacyEntity = diplomacyEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        foreach (var (factionEntity, faction) in world.Query<FactionComponent>())
        {
            if (faction.IsPlayerControlled) continue;

            TryPlaceUnits(world, factionEntity);
            RunMovement(world, factionEntity);
            TryLaunchNuclear(world, factionEntity);
        }
    }

    private void TryPlaceUnits(World world, Entity faction)
    {
        var budget = world.Get<PlacementBudgetComponent>(faction);
        if (budget == null) return;

        int unitCount = world.Query<UnitComponent, OwnershipComponent>().Count(t => t.Item3.Owner == faction);
        if (unitCount >= MaxAiUnits) return;

        Entity? homeTerritory = null;
        foreach (var (territoryEntity, territory, ownership) in world.Query<TerritoryComponent, OwnershipComponent>())
        {
            if (territory.Kind == TerritoryKind.Land && ownership.Owner == faction)
            {
                homeTerritory = territoryEntity;
                break;
            }
        }
        if (homeTerritory == null) return;

        var homeInfo = world.Get<TerritoryComponent>(homeTerritory.Value)!;

        foreach (var defId in PlacementPriority)
        {
            var def = UnitDefinitions.All[defId];
            if (budget.Points < def.PlacementCost) continue;

            int existingOfType = world.Query<UnitComponent, OwnershipComponent>()
                .Count(t => t.Item3.Owner == faction && t.Item2.DefId == defId);
            if (existingOfType >= MaxPerType[defId]) continue;

            double lat, lon;
            if (def.Domain == UnitDomain.Land)
            {
                lat = homeInfo.Latitude;
                lon = homeInfo.Longitude;
            }
            else
            {
                var seaPoint = FindSeaSpawnPoint(world, homeInfo);
                if (seaPoint == null) continue;
                (lat, lon) = seaPoint.Value;
            }

            if (!PlacementValidator.CanPlace(world, _terrainMap, def.Domain, faction, lat, lon, out _)) continue;

            UnitFactory.Spawn(world, defId, faction, lat, lon);
            budget.Points -= def.PlacementCost;
            return; // one placement per tick - gradual buildup, not an instant dump
        }
    }

    private (double lat, double lon)? FindSeaSpawnPoint(World world, TerritoryComponent homeInfo)
    {
        Entity? nearestSea = null;
        double bestDistance = double.MaxValue;

        foreach (var (territoryEntity, territory) in world.Query<TerritoryComponent>())
        {
            if (territory.Kind != TerritoryKind.Sea) continue;

            double distanceKm = GeoMath.HaversineDistanceKm(homeInfo.Latitude, homeInfo.Longitude, territory.Latitude, territory.Longitude);
            if (distanceKm < bestDistance)
            {
                bestDistance = distanceKm;
                nearestSea = territoryEntity;
            }
        }

        if (nearestSea == null) return null;
        var seaInfo = world.Get<TerritoryComponent>(nearestSea.Value)!;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            double candidateLat = seaInfo.Latitude + (_random.NextDouble() * 2 - 1) * 5;
            double candidateLon = seaInfo.Longitude + (_random.NextDouble() * 2 - 1) * 5;
            if (_terrainMap.IsSea(candidateLat, candidateLon))
                return (candidateLat, candidateLon);
        }

        return (seaInfo.Latitude, seaInfo.Longitude); // fallback: the sea zone's own anchor point, guaranteed water
    }

    private void RunMovement(World world, Entity faction)
    {
        var units = world.Query<UnitComponent, PositionComponent, OwnershipComponent, MovementComponent>()
            .Where(t => t.Item4.Owner == faction)
            .ToList();

        foreach (var (unitEntity, unitInfo, position, ownership, movement) in units)
        {
            if (world.Has<MoveOrderComponent>(unitEntity)) continue;

            Entity? target = null;
            double bestDistance = double.MaxValue;

            foreach (var (enemyEntity, enemyDetection, enemyPos, enemyOwnership) in world.Query<DetectionComponent, PositionComponent, OwnershipComponent>())
            {
                if (enemyOwnership.Owner == faction) continue;
                if (!enemyDetection.DetectedByFactions.Contains(faction)) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, enemyPos.Latitude, enemyPos.Longitude);
                if (distanceKm < bestDistance)
                {
                    bestDistance = distanceKm;
                    target = enemyEntity;
                }
            }

            double targetLat, targetLon;
            if (target.HasValue)
            {
                var targetPos = world.Get<PositionComponent>(target.Value)!;
                targetLat = targetPos.Latitude;
                targetLon = targetPos.Longitude;
            }
            else
            {
                targetLat = position.Latitude + (_random.NextDouble() * 2 - 1) * 5;
                targetLon = position.Longitude + (_random.NextDouble() * 2 - 1) * 5;

                bool validPatrol = unitInfo.Domain == UnitDomain.Land
                    ? _terrainMap.IsLand(targetLat, targetLon)
                    : _terrainMap.IsSea(targetLat, targetLon);
                if (!validPatrol) continue;
            }

            world.Set(unitEntity, new MoveOrderComponent { TargetLatitude = targetLat, TargetLongitude = targetLon });
        }
    }

    private void TryLaunchNuclear(World world, Entity faction)
    {
        var defcon = world.Get<DefconComponent>(_defconEntity);
        if (defcon == null || defcon.Level > 1) return;
        var diplomacy = world.Get<DiplomacyComponent>(_diplomacyEntity);

        foreach (var (launcherEntity, weapon, position, ownership) in world.Query<WeaponComponent, PositionComponent, OwnershipComponent>())
        {
            if (ownership.Owner != faction || !weapon.IsNuclear) continue;
            if (weapon.CooldownRemaining > 0) continue;

            var logistics = world.Get<LogisticsComponent>(launcherEntity);
            if (logistics != null && logistics.Ammo < weapon.AmmoPerShot) continue;

            Entity? targetCity = null;
            double bestDistance = weapon.RangeKm;

            foreach (var (cityEntity, city, cityPosition) in world.Query<CityComponent, PositionComponent>())
            {
                var cityOwnership = world.Get<OwnershipComponent>(city.ParentTerritory);
                if (cityOwnership?.Owner is not { } cityFaction || cityFaction == faction) continue;
                if (diplomacy != null && diplomacy.GetStance(faction, cityFaction) != RelationStance.War) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, cityPosition.Latitude, cityPosition.Longitude);
                if (distanceKm <= bestDistance)
                {
                    bestDistance = distanceKm;
                    targetCity = cityEntity;
                }
            }

            if (targetCity is { } city2)
            {
                var targetPosition = world.Get<PositionComponent>(city2)!;
                if (NuclearStrikeLauncher.TryLaunch(world, launcherEntity, targetPosition.Latitude, targetPosition.Longitude))
                    return; // one launch per tick per faction
            }
        }
    }
}