using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public enum MatchState { InProgress, PlayerWon, PlayerLost }

public class WinConditionSystem : ISystem
{
    private readonly Entity _playerFaction;
    private readonly Entity _chatterEntity;
    public MatchState State { get; private set; } = MatchState.InProgress;

    public WinConditionSystem(Entity playerFaction, Entity chatterEntity)
    {
        _playerFaction = playerFaction;
        _chatterEntity = chatterEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        if (State != MatchState.InProgress) return;

        // Population collapse: total population across all owned territory hitting zero
        // forfeits everything at once (societal collapse), separate from gradual capture.
        foreach (var (factionEntity, faction) in world.Query<FactionComponent>())
        {
            if (faction.IsEliminated) continue;

            var ownedTerritories = new List<Entity>();
            foreach (var (territoryEntity, territory, ownership) in world.Query<TerritoryComponent, OwnershipComponent>())
            {
                if (territory.Kind == TerritoryKind.Land && ownership.Owner == factionEntity)
                    ownedTerritories.Add(territoryEntity);
            }
            if (ownedTerritories.Count == 0) continue;

            double totalPopulation = 0;
            foreach (var (cityEntity, city, population) in world.Query<CityComponent, PopulationComponent>())
            {
                if (ownedTerritories.Contains(city.ParentTerritory))
                    totalPopulation += population.CurrentPopulation;
            }

            if (totalPopulation <= 0)
            {
                foreach (var territoryEntity in ownedTerritories)
                    world.Get<OwnershipComponent>(territoryEntity)!.Owner = null;
            }
        }

        // Elimination: zero territory left, whether from collapse above or from being
        // conquered piece by piece by CaptureSystem.
        foreach (var (factionEntity, faction) in world.Query<FactionComponent>())
        {
            if (faction.IsEliminated) continue;

            bool ownsAnyTerritory = false;
            foreach (var (territoryEntity, territory, ownership) in world.Query<TerritoryComponent, OwnershipComponent>())
            {
                if (territory.Kind == TerritoryKind.Land && ownership.Owner == factionEntity)
                {
                    ownsAnyTerritory = true;
                    break;
                }
            }

            if (!ownsAnyTerritory)
                faction.IsEliminated = true;
            ChatterLog.Post(world, _chatterEntity, $"{faction.Name} has been eliminated");
        }

        var playerInfo = world.Get<FactionComponent>(_playerFaction);
        if (playerInfo == null) return;

        if (playerInfo.IsEliminated)
        {
            State = MatchState.PlayerLost;
            ChatterLog.Post(world, _chatterEntity, "DEFEAT - all territory lost");
            return;
        }

        int aliveCount = 0;
        foreach (var (_, faction) in world.Query<FactionComponent>())
            if (!faction.IsEliminated) aliveCount++;

        if (aliveCount <= 1)
            State = MatchState.PlayerWon;
        ChatterLog.Post(world, _chatterEntity, "VICTORY - last faction standing");
    }
}