using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class NuclearImpactSystem : ISystem
{
    private const double UnitDamageMultiplier = 5.0;

    public void Update(World world, GameTime gameTime)
    {
        var arrived = new List<Entity>();

        foreach (var (entity, strike, position) in world.Query<IncomingStrikeComponent, PositionComponent>())
        {
            if (world.Has<MoveOrderComponent>(entity)) continue;
            arrived.Add(entity);
        }

        foreach (var missile in arrived)
        {
            var strike = world.Get<IncomingStrikeComponent>(missile)!;
            var position = world.Get<PositionComponent>(missile)!;

            // Casualties: cities within the blast radius.
            foreach (var (cityEntity, city, cityPosition, population) in world.Query<CityComponent, PositionComponent, PopulationComponent>())
            {
                double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, cityPosition.Latitude, cityPosition.Longitude);
                if (distanceKm > strike.BlastRadiusKm) continue;

                double falloff = 1.0 - distanceKm / strike.BlastRadiusKm;
                int casualties = (int)(strike.MaxCasualties * falloff);
                if (casualties > 0)
                    CasualtyTracker.Apply(world, cityEntity, casualties, strike.AttackerFaction);
            }

            // Military units/installations within the blast radius take heavy damage too.
            var destroyed = new List<Entity>();
            foreach (var (targetEntity, targetPosition, health) in world.Query<PositionComponent, HealthComponent>())
            {
                double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, targetPosition.Latitude, targetPosition.Longitude);
                if (distanceKm > strike.BlastRadiusKm) continue;

                double falloff = 1.0 - distanceKm / strike.BlastRadiusKm;
                health.CurrentHealth -= strike.MaxCasualties * falloff * UnitDamageMultiplier;

                if (health.CurrentHealth <= 0)
                    destroyed.Add(targetEntity);
            }

            foreach (var entity in destroyed)
                world.DestroyEntity(entity);

            world.DestroyEntity(missile);
        }
    }
}