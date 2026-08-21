using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System;

namespace WorldNMilSim.Systems;

public class LogisticsSystem : ISystem
{
    // Placeholder pacing: ~50 seconds to refill from empty while in supply. Tune later.
    private const double SupplyRegenFractionPerSecond = 0.02;

    public void Update(World world, GameTime gameTime)
    {
        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds * Core.SimulationClock.TimeScale;

        foreach (var (unit, logistics, position, ownership) in world.Query<LogisticsComponent, PositionComponent, OwnershipComponent>())
        {
            if (ownership.Owner is not { } faction) continue;
            if (logistics.MaxFuel <= 0 && logistics.MaxAmmo <= 0) continue; // e.g. radar station: nothing to track

            var nearest = SupplyGraph.NearestTerritory(world, position.Latitude, position.Longitude);
            var distance = SupplyGraph.DistanceToNearestSupplySource(world, nearest, faction, Math.Max(logistics.SupplyRangeKm, 1));

            logistics.IsSupplied = distance.HasValue;

            if (logistics.IsSupplied)
            {
                logistics.Fuel = Math.Min(logistics.MaxFuel, logistics.Fuel + logistics.MaxFuel * SupplyRegenFractionPerSecond * dtSeconds);
                logistics.Ammo = Math.Min(logistics.MaxAmmo, logistics.Ammo + logistics.MaxAmmo * SupplyRegenFractionPerSecond * dtSeconds);
            }
            else if (logistics.MaxFuel > 0)
            {
                logistics.Fuel = Math.Max(0, logistics.Fuel - logistics.FuelConsumptionPerHour / 3600.0 * dtSeconds);
            }
        }
    }
}