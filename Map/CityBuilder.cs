using System.Collections.Generic;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Map;

public static class CityBuilder
{
    public static List<Entity> Build(World world, Dictionary<string, Entity> territories)
    {
        var cities = new List<Entity>();

        foreach (var def in CityDefinition.Cities)
        {
            var territoryEntity = territories[def.TerritoryId];

            var e = world.CreateEntity();
            world.Set(e, new CityComponent { Name = def.Name, ParentTerritory = territoryEntity, IsCapital = def.IsCapital });
            world.Set(e, new PositionComponent { Latitude = def.Latitude, Longitude = def.Longitude });
            world.Set(e, new PopulationComponent { MaxPopulation = def.MaxPopulation, CurrentPopulation = def.MaxPopulation });

            cities.Add(e);
        }

        return cities;
    }
}