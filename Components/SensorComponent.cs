using System.Collections.Generic;

namespace WorldNMilSim.Components;

public enum SensorType { Radar, Sonar }
public enum SonarMode { Passive, Active }

public class Sensor
{
    public required SensorType Type;
    public double DetectionRadiusKm;
    public double ActiveBonusRadiusKm;
    public SonarMode Mode = SonarMode.Passive;
    public double FacingDegrees;              // 0 = North, 90 = East, etc. - only meaningful if FieldOfViewDegrees < 360
    public double FieldOfViewDegrees = 360;    // 360 = omnidirectional (default for everything except Radar Station)

    public double EffectiveRangeKm =>
        Type == SensorType.Sonar && Mode == SonarMode.Active
            ? DetectionRadiusKm + ActiveBonusRadiusKm
            : DetectionRadiusKm;
}

public class SensorsComponent
{
    public List<Sensor> Sensors = new();
}