namespace WorldNMilSim.Components;

public class InterceptorComponent
{
    public double RangeKm;
    public double InterceptChance; // 0-1, per attempt
    public int RemainingInterceptors;
    public int MaxInterceptors;
    public double CooldownSeconds;
    public double CooldownRemaining;
}