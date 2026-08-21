using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class MovementSystem : ISystem
{
    private const double ArrivalThresholdKm = 5.0;

    public void Update(World world, GameTime gameTime)
    {
        double dtHours = gameTime.ElapsedGameTime.TotalHours * Core.SimulationClock.TimeScale;

        foreach (var (entity, movement, position, order) in world.Query<MovementComponent, PositionComponent, MoveOrderComponent>())
        {
            double distanceKm = GeoMath.HaversineDistanceKm(position.Latitude, position.Longitude, order.TargetLatitude, order.TargetLongitude);
            double travelKm = movement.MaxSpeedKmh * dtHours;

            if (travelKm >= distanceKm || distanceKm <= ArrivalThresholdKm)
            {
                position.Latitude = order.TargetLatitude;
                position.Longitude = order.TargetLongitude;
                world.Remove<MoveOrderComponent>(entity);
                continue;
            }

            double bearing = GeoMath.InitialBearingRadians(position.Latitude, position.Longitude, order.TargetLatitude, order.TargetLongitude);
            var (newLat, newLon) = GeoMath.DestinationPoint(position.Latitude, position.Longitude, bearing, travelKm);
            position.Latitude = newLat;
            position.Longitude = newLon;
        }
    }
}