using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public class CaptureStateComponent
{
    public Entity? CapturingFaction;
    public double Progress; // 0 to 1
}