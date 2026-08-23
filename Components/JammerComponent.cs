namespace WorldNMilSim.Components;

public class JammerComponent
{
    public double JamRangeKm;
    public double JamStrength; // 0-1, radar signature reduction for friendly units within range
    public bool IsActive;
}