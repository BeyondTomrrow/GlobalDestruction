using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class DetectionSystem : ISystem
{
    private const double EmconSignatureMultiplier = 0.5;

    public void Update(World world, GameTime gameTime)
    {
        foreach (var (_, detection) in world.Query<DetectionComponent>())
            detection.DetectedByFactions.Clear();

        foreach (var (detectorEntity, sensors, detectorPos, detectorOwnership) in world.Query<SensorsComponent, PositionComponent, OwnershipComponent>())
        {
            if (detectorOwnership.Owner is not { } detectorFaction) continue;
            if (sensors.Sensors.Count == 0) continue;

            bool detectorEmcon = world.Has<EmconComponent>(detectorEntity);

            foreach (var (targetEntity, detection, targetPos, targetOwnership, stealth, targetUnit) in world.Query<DetectionComponent, PositionComponent, OwnershipComponent, StealthComponent, UnitComponent>())
            {
                if (targetEntity == detectorEntity) continue;
                if (targetOwnership.Owner is not { } targetFaction || targetFaction == detectorFaction) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(
                    detectorPos.Latitude, detectorPos.Longitude,
                    targetPos.Latitude, targetPos.Longitude);

                bool targetEmcon = world.Has<EmconComponent>(targetEntity);

                foreach (var sensor in sensors.Sensors)
                {
                    if (sensor.Type == SensorType.Radar && detectorEmcon) continue;

                    double domainMultiplier = SensorEffectivenessAgainst(sensor.Type, targetUnit.Domain);
                    if (domainMultiplier <= 0) continue;

                    if (sensor.Type == SensorType.Radar && sensor.FieldOfViewDegrees < 360)
                    {
                        double bearingDeg = GeoMath.InitialBearingRadians(
                            detectorPos.Latitude, detectorPos.Longitude,
                            targetPos.Latitude, targetPos.Longitude) * (180.0 / System.Math.PI);
                        if (bearingDeg < 0) bearingDeg += 360;
                        if (!IsWithinArc(bearingDeg, sensor.FacingDegrees, sensor.FieldOfViewDegrees)) continue;
                    }

                    double targetSignature = sensor.Type == SensorType.Radar
                        ? GetEffectiveRadarSignature(world, targetEntity, targetFaction, targetPos.Latitude, targetPos.Longitude, stealth)
                        : GetEffectiveAcousticSignature(world, targetEntity, stealth);

                    if (targetEmcon)
                        targetSignature *= EmconSignatureMultiplier;

                    double effectiveRangeKm = sensor.EffectiveRangeKm * domainMultiplier * targetSignature;

                    if (distanceKm <= effectiveRangeKm)
                    {
                        detection.DetectedByFactions.Add(detectorFaction);
                        break;
                    }
                }
            }
        }
    }

    private static double SensorEffectivenessAgainst(SensorType sensorType, UnitDomain targetDomain)
    {
        return (sensorType, targetDomain) switch
        {
            (SensorType.Radar, UnitDomain.Submerged) => 0.0,
            (SensorType.Sonar, UnitDomain.Land) => 0.0,
            (SensorType.Sonar, UnitDomain.Surface) => 0.6,
            _ => 1.0
        };
    }

    private static double GetEffectiveAcousticSignature(World world, Entity target, StealthComponent stealth)
    {
        var sensors = world.Get<SensorsComponent>(target);
        if (sensors != null)
        {
            foreach (var sensor in sensors.Sensors)
            {
                if (sensor.Type == SensorType.Sonar && sensor.Mode == SonarMode.Active)
                    return 1.0;
            }
        }
        return stealth.AcousticSignature;
    }

    private static double GetEffectiveRadarSignature(World world, Entity target, Entity targetFaction, double targetLat, double targetLon, StealthComponent stealth)
    {
        var jammer = world.Get<JammerComponent>(target);
        if (jammer != null && jammer.IsActive)
            return 1.0; // actively jamming makes you a beacon - home-on-jam risk

        double jamMultiplier = GetJamMultiplier(world, targetFaction, targetLat, targetLon);
        return stealth.RadarSignature * jamMultiplier;
    }

    private static double GetJamMultiplier(World world, Entity targetFaction, double targetLat, double targetLon)
    {
        double strongestJam = 0;

        foreach (var (jammerEntity, jammer, jammerPos, jammerOwnership) in world.Query<JammerComponent, PositionComponent, OwnershipComponent>())
        {
            if (jammerOwnership.Owner != targetFaction || !jammer.IsActive) continue;

            double distanceKm = GeoMath.HaversineDistanceKm(targetLat, targetLon, jammerPos.Latitude, jammerPos.Longitude);
            if (distanceKm > jammer.JamRangeKm) continue;

            if (jammer.JamStrength > strongestJam)
                strongestJam = jammer.JamStrength;
        }

        return 1.0 - strongestJam;
    }

    private static bool IsWithinArc(double bearingDegrees, double facingDegrees, double fovDegrees)
    {
        double diff = ((bearingDegrees - facingDegrees + 540) % 360) - 180;
        return System.Math.Abs(diff) <= fovDegrees / 2.0;
    }
}