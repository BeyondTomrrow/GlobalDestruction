using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class NuclearImpactSystem : ISystem
{
    public void Update(World world, GameTime gameTime)
    {
        var arrived = new List<Entity>();

        foreach (var (entity, strike, position) in world.Query<IncomingStrikeComponent, PositionComponent>())
        {
            if (world.Has<MoveOrderComponent>(entity)) continue; // still in flight
            arrived.Add(entity);
        }

        foreach (var missile in arrived)
        {
            var strike = world.Get<IncomingStrikeComponent>(missile)!;
            var position = world.Get<PositionComponent>(missile)!;

            foreach (var (cityEntity, city, cityPosition, population) in world.Query<CityComponent, PositionComponent, PopulationComponent>())
            {
                double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, cityPosition.Latitude, cityPosition.Longitude);
                if (distanceKm > strike.BlastRadiusKm) continue;

                double falloff = 1.0 - distanceKm / strike.BlastRadiusKm;
                int casualties = (int)(strike.MaxCasualties * falloff);
                if (casualties > 0)
                    CasualtyTracker.Apply(world, cityEntity, casualties, strike.AttackerFaction);
            }

            world.DestroyEntity(missile);
        }
    }
}