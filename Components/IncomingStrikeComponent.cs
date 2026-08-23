using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

// Attached to a missile in flight once it goes boom casualties happens.
public class IncomingStrikeComponent
{
    public Entity AttackerFaction;
    public double BlastRadiusKm;
    public double MaxCasualties;
    
}