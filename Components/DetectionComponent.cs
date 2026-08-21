using System.Collections.Generic;
using WorldNMilSim.Core;

namespace WorldNMilSim.Core;

// Which factions currently have this unit detected. Rebuilt every tick by DetectionSystem

public class DetectionComponent
{
    public HashSet<Entity> DetectedByFactions = new();
}