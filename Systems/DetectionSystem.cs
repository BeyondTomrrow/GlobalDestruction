using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System.Linq;

namespace WorldNMilSim.Systems;

public class DetectionSystem : ISystem
{
    public void Update(World world, GameTime gameTime)
    {
        foreach (var (_, detection) in world.Query<DetectionComponent>())
            detection.DetectedByFactions.Clear();

        var detectors = world.Query<SensorComponent, PositionComponent, OwnershipComponent>().ToList();
        var targets = world.Query<DetectionComponent, PositionComponent, OwnershipComponent, StealthComponent>().ToList();

        foreach (var (detectorEntity, sensor, detectorPos, detectorOwnership) in detectors)
        {
            if (detectorOwnership.Owner is not { } detectorFaction) continue;

            foreach (var (targetEntity, detection, targetPos, targetOwnership, stealth) in targets)
            {
                if (targetEntity == detectorEntity) continue;
                if (targetOwnership.Owner is not { } targetFaction) continue;
                if (targetFaction == detectorFaction) continue; // no need to "detect" your own units

                double distanceKm = GeoMath.HaversineDistanceKm(
                    detectorPos.Latitude, detectorPos.Longitude,
                    targetPos.Latitude, targetPos.Longitude);

                double effectiveRangeKm = sensor.DetectionRadiusKm * stealth.SignatureFactor;

                if (distanceKm <= effectiveRangeKm)
                    detection.DetectedByFactions.Add(detectorFaction);
            }
        }
    }
}