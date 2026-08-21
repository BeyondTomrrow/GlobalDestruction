using System.Collections.Generic;
using WorldNMilSim.Core;

namespace WorldNMilSim.Components;

public enum RouteKind { Land, Sea }

public struct Route
{
    public Entity Target;
    public RouteKind Kind;
    public double DistanceKm;
}

public class AdjacencyComponent
{
    public List<Route> Routes = new();
}