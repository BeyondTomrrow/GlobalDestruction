using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class UnitFactory
{
    public static Entity Spawn(World world, string defId, Entity faction, double latitude, double longitude, double radarFacingDegrees = 0)
    {
        var def = UnitDefinitions.All[defId];
        var e = world.CreateEntity();

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

        world.Set(e, new UnitComponent { DefId = def.Id, Name = def.Name, Type = def.Type, Domain = def.Domain, IsStationary = def.IsStationary });
        world.Set(e, new StealthComponent { RadarSignature = def.RadarSignature, AcousticSignature = def.AcousticSignature });
        world.Set(e, new SensorsComponent { Sensors = SensorLoadouts.For(def.Type, radarFacingDegrees) });

        var jammer = JammerLoadouts.For(def.Type);
        if (jammer != null)
            world.Set(e, jammer);

        world.Set(e, new DetectionComponent());

        var decoyLauncher = DecoyLoadouts.For(def.Type);
        if(decoyLauncher != null)
            world.Set(e, decoyLauncher);

        if (def.WeaponRangeKm > 0)
            world.Set(e, new WeaponComponent
            {
                RangeKm = def.WeaponRangeKm,
                Damage = def.WeaponDamage,
                AmmoPerShot = 1,
                IsNuclear = def.IsNuclearCapable,
                RateOfFireSeconds = def.RateOfFireSeconds,
                CooldownRemaining = 0,
                BlastRadiusKm = def.BlastRadiusKm
            });

        if (def.MaxSpeedKmh > 0)
            world.Set(e, new MovementComponent { MaxSpeedKmh = def.MaxSpeedKmh });

        return e;
    }

    // Convenience for fixed installations built at a territory's own coordinates.
    public static Entity SpawnAtTerritory(World world, string defId, Entity faction, Entity territory, double radarFacingDegrees = 0)
    {
        var territoryInfo = world.Get<TerritoryComponent>(territory)!;
        return Spawn(world, defId, faction, territoryInfo.Latitude, territoryInfo.Longitude, radarFacingDegrees);
    }
}