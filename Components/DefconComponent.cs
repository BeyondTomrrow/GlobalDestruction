namespace WorldNMilSim.Components;

// Global game state, not per-faction  DEFCON level is shared by everyone same as the real thing.
public class DefconComponent
{
    public int Level = 5;
    public double ElapsedSeconds;
}