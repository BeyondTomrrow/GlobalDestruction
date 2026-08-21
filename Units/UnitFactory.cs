using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class UnitFactory
{
    public static Entity Spawn(World world, string defId, Entity faction, double latitude, double longitude)
    {
        var def = UnitDefinitions.All[defId];
        var e = world.CreateEntity();

        world.Set(e, new UnitComponent { DefId = def.Id, Name = def.Name, Type = def.Type, IsStationary = def.IsStationary });
        world.Set(e, new PositionComponent { Latitude = latitude, Longitude = longitude });
        world.Set(e, new OwnershipComponent { Owner = faction });
        world.Set(e, new HealthComponent { CurrentHealth = def.MaxHealth, MaxHealth = def.MaxHealth });

        world.Set(e, new LogisticsComponent
        {
            Fuel = def.MaxFuel,
            MaxFuel = def.MaxFuel,
            FuelConsumptionPerHour = def.FuelConsumptionPerHour,
            Ammo = def.MaxAmmo,
            MaxAmmo = def.MaxAmmo,
            SupplyRangeKm = def.SupplyRangeKm,
            IsSupplied = true
        });

        if (def.DetectionRadiusKm > 0)
            world.Set(e, new SensorComponent { DetectionRadiusKm = def.DetectionRadiusKm });

        world.Set(e, new StealthComponent { SignatureFactor = def.StealthSignature });
        world.Set(e, new DetectionComponent()); 

        if (def.WeaponRangeKm > 0)
            world.Set(e, new WeaponComponent
            {
                RangeKm = def.WeaponRangeKm,
                Damage = def.WeaponDamage,
                AmmoPerShot = 1,
                IsNuclear = def.IsNuclearCapable
            });

        if (def.MaxSpeedKmh > 0)
            world.Set(e, new MovementComponent { MaxSpeedKmh = def.MaxSpeedKmh });

        return e;
    }

    // Convenience for fixed installations built at a territory's own coordinates.
    public static Entity SpawnAtTerritory(World world, string defId, Entity faction, Entity territory)
    {
        var territoryInfo = world.Get<TerritoryComponent>(territory)!;
        return Spawn(world, defId, faction, territoryInfo.Latitude, territoryInfo.Longitude);
    }
}