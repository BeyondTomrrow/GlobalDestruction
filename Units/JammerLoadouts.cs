using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class JammerLoadouts
{
    public static JammerComponent? For(UnitType type) => type switch
    {
        UnitType.Carrier => new JammerComponent { JamRangeKm = 300, JamStrength = 0.6 },
        _ => null
    };
}