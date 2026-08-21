namespace WorldNMilSim.Components;

public enum UnitType { Silo, RadarStation, Airbase, Destroyer, Submarine, Carrier }

public class UnitComponent
{
    public required string DefId;
    public required string Name;
    public required UnitType Type;
    public bool IsStationary;
}