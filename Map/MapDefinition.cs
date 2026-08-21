using System.Collections.Generic;
using WorldNMilSim.Components;

namespace WorldNMilSim.Map;

public record TerritoryDef(string Id, string Name, TerritoryKind Kind, double Latitude, double Longitude);
public record RouteDef(string FromId, string ToId, RouteKind Kind);

public static class MapDefinition
{
    public static readonly List<TerritoryDef> Territories = new()
    {
      
        // Land regions - anchored to each region's largest/capital city
        new("na_west",     "North America - West",   TerritoryKind.Land,  39.74,  -104.99 ),  // Denver
        new("na_east",     "North America - East",   TerritoryKind.Land,  38.91,   -77.04 ),  // Washington D.C.
        new("s_america",   "South America",          TerritoryKind.Land, -15.83,   -47.92 ),  // Brasilia
        new("w_europe",    "Western Europe",         TerritoryKind.Land,  48.86,     2.35),  // Paris
        new("e_europe",    "Eastern Europe",         TerritoryKind.Land,  55.76,    37.62),  // Moscow
        new("siberia",     "Siberia",                TerritoryKind.Land,  55.01,    82.94),  // Novosibirsk
        new("middle_east", "Middle East",            TerritoryKind.Land,  33.32,    44.37),  // Baghdad
        new("n_africa",    "North Africa",           TerritoryKind.Land,  30.04,    31.24),  // Cairo
        new("ssa_africa",  "Sub-Saharan Africa",     TerritoryKind.Land,  -4.44,    15.27),  // Kinshasa
        new("s_asia",      "South Asia",             TerritoryKind.Land,  28.61,    77.21), // New Delhi
        new("e_asia",      "East Asia",              TerritoryKind.Land,  39.90,   116.41), // Beijing
        new("se_asia",     "Southeast Asia",         TerritoryKind.Land,  13.76,   100.50),  // Bangkok
        new("oceania",     "Oceania",                TerritoryKind.Land, -33.87,   151.21),  // Sydney
        new("central_america", "Central America",    TerritoryKind.Land, 19.43, -99.13), // Mexico City

        // Sea zones - kept as rough mid-ocean points, no city anchor needed
        new("sea_n_atlantic", "North Atlantic", TerritoryKind.Sea,  40,  -40),
        new("sea_s_atlantic", "South Atlantic", TerritoryKind.Sea, -30,  -20),
        new("sea_mediterranean", "Mediterranean", TerritoryKind.Sea, 35,   18),
        new("sea_n_pacific",  "North Pacific",  TerritoryKind.Sea,  40, -170),
        new("sea_s_pacific",  "South Pacific",  TerritoryKind.Sea, -20, -140),
        new("sea_indian",     "Indian Ocean",   TerritoryKind.Sea, -20,   75),
        new("sea_arctic",     "Arctic Ocean",   TerritoryKind.Sea,  80,    0),

    };

    public static readonly List<RouteDef> Routes = new()
    {
        // Land - Land
        new("na_west", "na_east", RouteKind.Land),
        new("w_europe", "e_europe", RouteKind.Land),
        new("e_europe", "siberia", RouteKind.Land),
        new("e_europe", "middle_east", RouteKind.Land),
        new("middle_east", "n_africa", RouteKind.Land),
        new("middle_east", "s_asia", RouteKind.Land),
        new("n_africa", "ssa_africa", RouteKind.Land),
        new("s_asia", "e_asia", RouteKind.Land),
        new("s_asia", "se_asia", RouteKind.Land),
        new("e_asia", "se_asia", RouteKind.Land),
        new("e_asia", "siberia", RouteKind.Land),
        new("na_west", "central_america", RouteKind.Land),
        new("na_east", "central_america", RouteKind.Land),
        new("central_america", "s_america", RouteKind.Land),


        // Land - Sea (coastal access)
        new("central_america", "sea_n_atlantic", RouteKind.Sea),
        new("central_america", "sea_n_pacific", RouteKind.Sea),
        new("na_west", "sea_n_pacific", RouteKind.Sea),
        new("na_west", "sea_arctic", RouteKind.Sea),
        new("na_east", "sea_n_atlantic", RouteKind.Sea),
        new("na_east", "sea_arctic", RouteKind.Sea),
        new("s_america", "sea_s_atlantic", RouteKind.Sea),
        new("s_america", "sea_s_pacific", RouteKind.Sea),
        new("w_europe", "sea_n_atlantic", RouteKind.Sea),
        new("w_europe", "sea_mediterranean", RouteKind.Sea),
        new("w_europe", "sea_arctic", RouteKind.Sea),
        new("e_europe", "sea_arctic", RouteKind.Sea),
        new("siberia", "sea_arctic", RouteKind.Sea),
        new("siberia", "sea_n_pacific", RouteKind.Sea),
        new("middle_east", "sea_mediterranean", RouteKind.Sea),
        new("middle_east", "sea_indian", RouteKind.Sea),
        new("n_africa", "sea_mediterranean", RouteKind.Sea),
        new("n_africa", "sea_n_atlantic", RouteKind.Sea),
        new("ssa_africa", "sea_s_atlantic", RouteKind.Sea),
        new("ssa_africa", "sea_indian", RouteKind.Sea),
        new("s_asia", "sea_indian", RouteKind.Sea),
        new("e_asia", "sea_n_pacific", RouteKind.Sea),
        new("se_asia", "sea_indian", RouteKind.Sea),
        new("se_asia", "sea_s_pacific", RouteKind.Sea),
        new("oceania", "sea_indian", RouteKind.Sea),
        new("oceania", "sea_s_pacific", RouteKind.Sea),

        // Sea - Sea
        new("sea_n_atlantic", "sea_s_atlantic", RouteKind.Sea),
        new("sea_n_atlantic", "sea_arctic", RouteKind.Sea),
        new("sea_n_atlantic", "sea_mediterranean", RouteKind.Sea),
        new("sea_s_atlantic", "sea_indian", RouteKind.Sea),
        new("sea_n_pacific", "sea_s_pacific", RouteKind.Sea),
        new("sea_n_pacific", "sea_arctic", RouteKind.Sea),
        new("sea_s_pacific", "sea_indian", RouteKind.Sea),
        new("sea_indian", "sea_mediterranean", RouteKind.Sea),
    };
}