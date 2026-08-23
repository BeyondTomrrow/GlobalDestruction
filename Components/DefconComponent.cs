using System.ComponentModel;

namespace WorldNMilSim.Components;

// Global game state, not per-faction  DEFCON level is shared by everyone same as the real thing.
public class DefconComponent
{
    public int Level = 5;
    public double ProgressToNextLevel; //0-1 fills at a rate driven by current tension
}