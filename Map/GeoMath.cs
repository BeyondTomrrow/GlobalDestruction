using System;
using Microsoft.Xna.Framework;

namespace WorldNMilSim.Map;

public static class GeoMath
{
    public const double EarthRadiusKm = 6371.0;

    public static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;

    // Simple equirectangular projection - good enough until/unless we want a fancier map projection.
    public static Vector2 Project(double latitude, double longitude)
{
    float x = (float)((longitude + 180.0) / 360.0 * MapSpace.WIDTH);
    float y = (float)((90.0 - latitude) / 180.0 * MapSpace.HEIGHT);
    return new Vector2(x, y);
}
}