using System.Collections.Generic;
using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class SensorLoadouts
{
    public static List<Sensor> For(UnitType type, double radarFacingDegrees = 0) => type switch
    {
        UnitType.Silo => new List<Sensor> { new() { Type = SensorType.Radar, DetectionRadiusKm = 50 } },
        UnitType.RadarStation => new List<Sensor>
        {
            new() { Type = SensorType.Radar, DetectionRadiusKm = 1500, FacingDegrees = radarFacingDegrees, FieldOfViewDegrees = 120 },
        },
        UnitType.Airbase => new List<Sensor> { new() { Type = SensorType.Radar, DetectionRadiusKm = 300 } },
        UnitType.Destroyer => new List<Sensor>
        {
            new() { Type = SensorType.Radar, DetectionRadiusKm = 250 },
            new() { Type = SensorType.Sonar, DetectionRadiusKm = 60, ActiveBonusRadiusKm = 100 },
        },
        UnitType.Submarine => new List<Sensor>
        {
            new() { Type = SensorType.Sonar, DetectionRadiusKm = 150, ActiveBonusRadiusKm = 100 },
        },
        UnitType.Carrier => new List<Sensor>
        {
            new() { Type = SensorType.Radar, DetectionRadiusKm = 700 },
            new() { Type = SensorType.Sonar, DetectionRadiusKm = 40 },
        },
        _ => new List<Sensor>()
    };
}