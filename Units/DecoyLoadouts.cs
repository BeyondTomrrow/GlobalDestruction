using WorldNMilSim.Components;

namespace WorldNMilSim.Units;

public static class DecoyLoadouts
{
    public static DecoyLauncherComponent? For(UnitType type) => type switch
    {
        UnitType.Destroyer => new DecoyLauncherComponent { MaxDecoys = 6, RemainingDecoys = 6, CooldownSeconds = 8 },
        UnitType.Carrier => new DecoyLauncherComponent { MaxDecoys = 8, RemainingDecoys = 8, CooldownSeconds = 8 },
        UnitType.Submarine => new DecoyLauncherComponent { MaxDecoys = 4, RemainingDecoys = 4, CooldownSeconds = 10 },
        _ => null
    };
}