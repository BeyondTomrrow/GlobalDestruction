using Microsoft.Xna.Framework;

namespace WorldNMilSim.Components;

public class FactionComponent
{
    public required string Name;
    public Color Color;
    public bool IsPlayerControlled;
    public int TotalCasualtiesInflicted;
    public int TotalCasualtiesSuffered;
    public bool IsEliminated;
}