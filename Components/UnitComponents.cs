namespace WorldNMilSim.Components;

public enum UnitType { Silo, RadarStation, Airbase, Destroyer, Submarine, Carrier, Decoy, Army }
public enum UnitDomain { Land, Surface, Submerged }

public class UnitComponent
{
    public required string DefId;
    public required string Name;
    public required UnitType Type;
    public required UnitDomain Domain;
    public bool IsStationary;
}