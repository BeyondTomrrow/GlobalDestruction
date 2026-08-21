using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public class CityComponent
{
    public required string Name;
    public Entity ParentTerritory;
    public bool IsCapital;
}