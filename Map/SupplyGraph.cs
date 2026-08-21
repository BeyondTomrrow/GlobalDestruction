using WorldNMilSim.Core;
using WorldNMilSim.Components;
using System.Collections.Generic;

namespace WorldNMilSim.Map;

public static class SupplyGraph
{
    public static Entity NearestTerritory(World world, double latitude, double longitude)
    {
        Entity best = default;
        double bestDist = double.MaxValue;

        foreach (var (entity, territory) in world.Query<TerritoryComponent>())
        {
            double d = GeoMath.HaversineDistanceKm(latitude, longitude, territory.Latitude, territory.Longitude);
            if (d < bestDist)
            {
                bestDist = d;
                best = entity;
            }
        }

        return best;
    }

    // Shortest distance (km) from 'start' to the nearest Land territory owned by 'faction',
    // traveling freely through Sea zones but only through Land territories owned by 'faction'.
    // Returns null if no such path exists within maxDistanceKm.
    public static double? DistanceToNearestSupplySource(World world, Entity start, Entity faction, double maxDistanceKm)
    {
        var startTerritory = world.Get<TerritoryComponent>(start);
        var startOwnership = world.Get<OwnershipComponent>(start);
        if (startTerritory is { Kind: TerritoryKind.Land } && startOwnership?.Owner == faction)
            return 0;

        var best = new Dictionary<int, double> { [start.Id] = 0 };
        var queue = new PriorityQueue<Entity, double>();
        queue.Enqueue(start, 0);

        while (queue.TryDequeue(out var current, out var dist))
        {
            if (dist > best.GetValueOrDefault(current.Id, double.MaxValue)) continue;
            if (dist > maxDistanceKm) continue;

            var adjacency = world.Get<AdjacencyComponent>(current);
            if (adjacency == null) continue;

            foreach (var route in adjacency.Routes)
            {
                var neighborTerritory = world.Get<TerritoryComponent>(route.Target)!;
                var neighborOwnership = world.Get<OwnershipComponent>(route.Target);

                bool passable = neighborTerritory.Kind == TerritoryKind.Sea
                    || neighborOwnership?.Owner == faction;
                if (!passable) continue;

                double newDist = dist + route.DistanceKm;
                if (newDist > maxDistanceKm) continue;

                if (newDist < best.GetValueOrDefault(route.Target.Id, double.MaxValue))
                {
                    best[route.Target.Id] = newDist;

                    if (neighborTerritory.Kind == TerritoryKind.Land && neighborOwnership?.Owner == faction)
                        return newDist;

                    queue.Enqueue(route.Target, newDist);
                }
            }
        }

        return null;
    }
}