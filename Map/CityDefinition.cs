using System.Collections.Generic;

namespace WorldNMilSim.Map;

public record CityDef(string TerritoryId, string Name, double Latitude, double Longitude, int MaxPopulation, bool IsCapital);

public static class CityDefinition
{
    public static readonly List<CityDef> Cities = new()
    {
        new("na_west", "Denver", 39.74, -104.99, 25, true),
        new("na_west", "Los Angeles", 34.05, -118.24, 45, false),

        new("na_east", "Washington D.C.", 38.91, -77.04, 25, true),
        new("na_east", "New York", 40.71, -74.01, 65, false),

        new("central_america", "Mexico City", 19.43, -99.13, 35, true),
        new("central_america", "Havana", 23.11, -82.37, 10, false),

        new("s_america", "Brasilia", -15.83, -47.92, 15, true),
        new("s_america", "Buenos Aires", -34.60, -58.38, 55, false),

        new("w_europe", "Paris", 48.86, 2.35, 45, true),
        new("w_europe", "London", 51.51, -0.13, 50, false),

        new("e_europe", "Moscow", 55.76, 37.62, 45, true),
        new("e_europe", "Kyiv", 50.45, 30.52, 15, false),

        new("siberia", "Novosibirsk", 55.01, 82.94, 10, true),
        new("siberia", "Vladivostok", 43.12, 131.89, 5, false),

        new("middle_east", "Baghdad", 33.32, 44.37, 20, true),
        new("middle_east", "Tehran", 35.69, 51.39, 20, false),

        new("n_africa", "Cairo", 30.04, 31.24, 25, true),
        new("n_africa", "Algiers", 36.75, 3.06, 5, false),

        new("ssa_africa", "Kinshasa", -4.44, 15.27, 30, true),
        new("ssa_africa", "Lagos", 6.52, 3.38, 20, false),

        new("s_asia", "New Delhi", 28.61, 77.21, 70, true),
        new("s_asia", "Mumbai", 19.08, 72.88, 70, false),

        new("e_asia", "Beijing", 39.90, 116.41, 70, true),
        new("e_asia", "Shanghai", 31.23, 121.47, 70, false),

        new("se_asia", "Bangkok", 13.76, 100.50, 35, true),
        new("se_asia", "Jakarta", -6.21, 106.85, 30, false),

        new("oceania", "Sydney", -33.87, 151.21, 15, true),
        new("oceania", "Melbourne", -37.81, 144.96, 15, false),
    };
}