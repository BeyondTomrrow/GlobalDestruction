using WorldNMilSim.Core;
using WorldNMilSim.Components;
using System.Collections.Generic;

namespace WorldNMilSim.Map;

public static class MapBuilder
{
    // Creates one entity per territory/sea zone, wires up adjacency, returns an id -> Entity lookup.
    public static Dictionary<string, Entity> Build(World world)
    {
        var lookup = new Dictionary<string, Entity>();

        foreach (var def in MapDefinition.Territories)
        {
            var e = world.CreateEntity();

            world.Set(e, new TerritoryComponent
            {
                Id = def.Id,
                Name = def.Name,
                Kind = def.Kind,
                Latitude = def.Latitude,
                Longitude = def.Longitude
            });

            world.Set(e, new OwnershipComponent { Owner = null });
            world.Set(e, new AdjacencyComponent());

            if (def.Kind == TerritoryKind.Land && def.MaxPopulation > 0)
            {
                world.Set(e, new PopulationComponent
                {
                    MaxPopulation = def.MaxPopulation,
                    CurrentPopulation = def.MaxPopulation
                });
            }

            lookup[def.Id] = e;
        }

        foreach (var route in MapDefinition.Routes)
        {
            var from = lookup[route.FromId];
            var to = lookup[route.ToId];

            var fromInfo = world.Get<TerritoryComponent>(from)!;
            var toInfo = world.Get<TerritoryComponent>(to)!;
            double distanceKm = GeoMath.HaversineDistanceKm(
                fromInfo.Latitude, fromInfo.Longitude,
                toInfo.Latitude, toInfo.Longitude);

            world.Get<AdjacencyComponent>(from)!.Routes.Add(new Route { Target = to, Kind = route.Kind, DistanceKm = distanceKm });
            world.Get<AdjacencyComponent>(to)!.Routes.Add(new Route { Target = from, Kind = route.Kind, DistanceKm = distanceKm });
        }

        return lookup;
    }
}