using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class NuclearStrikeLauncher
{
    public static bool TryLaunch(World world, Entity launcher, double targetLat, double targetLon)
    {
        var weapon = world.Get<WeaponComponent>(launcher);
        var ownership = world.Get<OwnershipComponent>(launcher);
        var position = world.Get<PositionComponent>(launcher);
        var logistics = world.Get<LogisticsComponent>(launcher);

        if (weapon == null || !weapon.IsNuclear) return false;
        if (weapon.CooldownRemaining > 0) return false;
        if (logistics != null && logistics.Ammo < weapon.AmmoPerShot) return false;
        if (ownership?.Owner is not { } faction) return false;
        if (position == null) return false;

        if (logistics != null) logistics.Ammo -= weapon.AmmoPerShot;
        weapon.CooldownRemaining = weapon.RateOfFireSeconds;

        var missile = world.CreateEntity();
        world.Set(missile, new PositionComponent { Latitude = position.Latitude, Longitude = position.Longitude });
        world.Set(missile, new MovementComponent { MaxSpeedKmh = 7500 });
        world.Set(missile, new MoveOrderComponent { TargetLatitude = targetLat, TargetLongitude = targetLon });
        world.Set(missile, new IncomingStrikeComponent
        {
            AttackerFaction = faction,
            BlastRadiusKm = weapon.BlastRadiusKm,
            MaxCasualties = weapon.Damage
        });

        return true;
    }
}