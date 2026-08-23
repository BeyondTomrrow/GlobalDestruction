using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class InterceptSystem : ISystem
{
    private readonly System.Random _random = new();

    public void Update(World world, GameTime gameTime)
    {
        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds; // real time - matches other fast/dramatic systems

        var missiles = world.Query<IncomingStrikeComponent, PositionComponent>().ToList();

        foreach (var (interceptorEntity, interceptor, interceptorPos, ownership) in world.Query<InterceptorComponent, PositionComponent, OwnershipComponent>())
        {
            if (interceptor.CooldownRemaining > 0)
                interceptor.CooldownRemaining -= dtSeconds;

            if (ownership.Owner is not { } faction) continue;
            if (interceptor.RemainingInterceptors <= 0 || interceptor.CooldownRemaining > 0) continue;

            foreach (var (missileEntity, strike, missilePos) in missiles)
            {
                if (strike.AttackerFaction == faction) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(interceptorPos.Latitude, interceptorPos.Longitude, missilePos.Latitude, missilePos.Longitude);
                if (distanceKm > interceptor.RangeKm) continue;

                interceptor.RemainingInterceptors--;
                interceptor.CooldownRemaining = interceptor.CooldownSeconds;

                if (_random.NextDouble() < interceptor.InterceptChance)
                    world.DestroyEntity(missileEntity);

                break; // one attempt per tick per interceptor
            }
        }
    }
}