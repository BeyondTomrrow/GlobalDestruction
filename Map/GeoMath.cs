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

    private static double ToDeg(double rad) => rad * 180.0 / Math.PI;

    public static double InitialBearingRadians(double lat1, double lon1, double lat2, double lon2)
    {
        double phi1 = ToRad(lat1), phi2 = ToRad(lat2);
        double deltaLon = ToRad(lon2 - lon1);

        double y = Math.Sin(deltaLon) * Math.Cos(phi2);
        double x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(deltaLon);
        return Math.Atan2(y, x);
    }

    public static (double Latitude, double Longitude) DestinationPoint(double lat, double lon, double bearingRadians, double distanceKm)
    {
        double angularDistance = distanceKm / EarthRadiusKm;
        double phi1 = ToRad(lat), lambda1 = ToRad(lon);

        double phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(angularDistance) + Math.Cos(phi1) * Math.Sin(angularDistance) * Math.Cos(bearingRadians));
        double lambda2 = lambda1 + Math.Atan2(
            Math.Sin(bearingRadians) * Math.Sin(angularDistance) * Math.Cos(phi1),
            Math.Cos(angularDistance) - Math.Sin(phi1) * Math.Sin(phi2));

        return (ToDeg(phi2), ToDeg(lambda2));
    }

    // Inverse of Project() - screen/world space back to lat/long, for turning a mouse click into a destination.
    public static (double Latitude, double Longitude) Unproject(Vector2 worldPosition)
    {
        double longitude = worldPosition.X / MapSpace.WIDTH * 360.0 - 180.0;
        double latitude = 90.0 - worldPosition.Y / MapSpace.HEIGHT * 180.0;
        return (latitude, longitude);
    }
}