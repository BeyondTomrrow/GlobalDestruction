namespace WorldNMilSim.Core;

// How much faster simulated time passes than real time - applies to slow real-world
// processes (unit travel, fuel/ammo resupply). This does not touch combat that runs real time. 300 = 5 in game minutes every second 
public static class SimulationClock
{
    public const float TimeScale = 300f;
}