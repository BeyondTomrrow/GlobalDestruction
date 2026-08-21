using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public class OwnershipComponent
{
    public Entity? Owner; // null = unclaimed/neutral
}