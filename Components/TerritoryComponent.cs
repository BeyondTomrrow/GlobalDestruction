namespace WorldNMilSim.Components;

public enum TerritoryKind { Land, Sea }

public class TerritoryComponent
{
    public required string Id;
    public required string Name;
    public required TerritoryKind Kind;

    public double Latitude;
    public double Longitude;
}