using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class CombatSystem : ISystem
{
    private readonly Entity _tensionEntity;
    private readonly Entity _diplomacyEntity;

    public CombatSystem(Entity tensionEntity, Entity diplomacyEntity)
    {
        _tensionEntity = tensionEntity;
        _diplomacyEntity = diplomacyEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        // Real elapsed time on purpose - rate of fire should feel real-time even though
        // travel/resupply run on the compressed SimulationClock.TimeScale.
        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        var diplomacy = world.Get<DiplomacyComponent>(_diplomacyEntity);

        var attackers = world.Query<WeaponComponent, PositionComponent, OwnershipComponent>().ToList();

        foreach (var (attackerEntity, weapon, attackerPos, attackerOwnership) in attackers)
        {
            if (weapon.CooldownRemaining > 0)
                weapon.CooldownRemaining -= dtSeconds;

            if (weapon.IsNuclear) continue; // handled by a separate, player-directed strike system later
            if (weapon.CooldownRemaining > 0) continue;
            if (attackerOwnership.Owner is not { } attackerFaction) continue;

            var logistics = world.Get<LogisticsComponent>(attackerEntity);
            if (logistics != null && logistics.Ammo < weapon.AmmoPerShot) continue;

            Entity? bestTarget = null;
            double bestDistance = weapon.RangeKm;

            foreach (var (targetEntity, targetPos, targetOwnership) in world.Query<PositionComponent, OwnershipComponent>())
            {
                if (targetOwnership.Owner is not { } targetFaction || targetFaction == attackerFaction) continue;
                if (diplomacy != null && diplomacy.GetStance(attackerFaction, targetFaction) != RelationStance.War) continue;

                var targetDetection = world.Get<DetectionComponent>(targetEntity);
                if (targetDetection == null || !targetDetection.DetectedByFactions.Contains(attackerFaction)) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(attackerPos.Latitude, attackerPos.Longitude, targetPos.Latitude, targetPos.Longitude);
                if (distanceKm <= bestDistance)
                {
                    bestDistance = distanceKm;
                    bestTarget = targetEntity;
                }
            }

            if (bestTarget is { } target)
            {
                var targetHealth = world.Get<HealthComponent>(target);
                if (targetHealth != null)
                    targetHealth.CurrentHealth -= weapon.Damage;

                if (logistics != null)
                    logistics.Ammo -= weapon.AmmoPerShot;

                weapon.CooldownRemaining = weapon.RateOfFireSeconds;
                IncreaseTension(world, 4);
            }
        }

        var dead = new List<Entity>();
        foreach (var (entity, health) in world.Query<HealthComponent>())
        {
            if (health.CurrentHealth <= 0)
                dead.Add(entity);
        }
        foreach (var entity in dead)
            world.DestroyEntity(entity);
    }

    private void IncreaseTension(World world, double amount)
    {
        var tension = world.Get<TensionComponent>(_tensionEntity);
        if (tension != null)
            tension.Tension = System.Math.Min(100, tension.Tension + amount);
    }
}