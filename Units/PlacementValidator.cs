using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Units;

public static class PlacementValidator
{
    private const double MaxLandDistanceKm = 300;

    public static bool CanPlace(World world, TerrainMap terrainMap, UnitDomain domain, Entity faction, double lat, double lon, out string reason)
    {
        reason = "";

        bool needsLand = domain == UnitDomain.Land;
        bool locationIsSea = terrainMap.IsSea(lat, lon);

        if (needsLand && locationIsSea) { reason = "Must be placed on land"; return false; }
        if (!needsLand && !locationIsSea) { reason = "Must be placed at sea"; return false; }

        if (!needsLand) return true; // ships can deploy anywhere at sea - global naval reach

        // Land installations must be built within territory you actually own.
        foreach (var (territoryEntity, territory, ownership) in world.Query<TerritoryComponent, OwnershipComponent>())
        {
            if (territory.Kind != TerritoryKind.Land || ownership.Owner != faction) continue;

            double distanceKm = GeoMath.HaversineDistanceKm(lat, lon, territory.Latitude, territory.Longitude);
            if (distanceKm <= MaxLandDistanceKm)
                return true;
        }

        reason = "Must be within your territory";
        return false;
    }
}