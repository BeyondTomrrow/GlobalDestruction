using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class InterceptorLoadouts
{
    public static InterceptorComponent? For(UnitType type) => type switch
    {
        UnitType.RadarStation => new InterceptorComponent
        {
            RangeKm = 800, InterceptChance = 0.4,
            RemainingInterceptors = 6, MaxInterceptors = 6,
            CooldownSeconds = 5
        },
        _ => null
    };
}