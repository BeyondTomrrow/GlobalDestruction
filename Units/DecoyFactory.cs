using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class DecoyFactory
{
    private const double DecoyLifetimeSeconds = 30;

    public static Entity Spawn(World world, Entity faction, double latitude, double longitude, UnitDomain sourceDomain)
    {
        var e = world.CreateEntity();

        world.Set(e, new PositionComponent { Latitude = latitude, Longitude = longitude });
        world.Set(e, new OwnershipComponent { Owner = faction });
        world.Set(e, new DetectionComponent());
        world.Set(e, new UnitComponent { DefId = "decoy", Name = "Decoy", Type = UnitType.Decoy, Domain = sourceDomain, IsStationary = true });
        world.Set(e, new HealthComponent { CurrentHealth = 1, MaxHealth = 1 }); // dies the instant anything actually hits it

        world.Set(e, new StealthComponent
        {
            RadarSignature = sourceDomain == UnitDomain.Surface ? 1.0 : 0.0,
            AcousticSignature = sourceDomain == UnitDomain.Submerged ? 1.0 : 0.0
        });

        world.Set(e, new DecoyComponent { RemainingSeconds = DecoyLifetimeSeconds });

        return e;
    }
}