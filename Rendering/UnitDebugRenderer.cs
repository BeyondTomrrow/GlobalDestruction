using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System.Collections.Generic;
using System;

namespace WorldNMilSim.Rendering;

public class UnitDebugRenderer
{
    private readonly Dictionary<UnitType, Texture2D> _icons;
    private readonly int _mapWidth;
    private readonly int _mapHeight;
    private readonly Texture2D _selectionRingTexture;
    private readonly Texture2D _pixel;

    private static readonly Dictionary<UnitType, IconShape> ShapeByType = new()
    {
        [UnitType.Silo] = IconShape.Diamond,
        [UnitType.RadarStation] = IconShape.RingOutline,
        [UnitType.Airbase] = IconShape.Triangle,
        [UnitType.Destroyer] = IconShape.Square,
        [UnitType.Submarine] = IconShape.Circle,
        [UnitType.Carrier] = IconShape.Square,
        [UnitType.Decoy] = IconShape.Cross,
    };

    private static readonly Dictionary<UnitType, int> SizeByType = new()
    {
        [UnitType.Silo] = 14,
        [UnitType.RadarStation] = 18,
        [UnitType.Airbase] = 14,
        [UnitType.Destroyer] = 10,
        [UnitType.Submarine] = 10,
        [UnitType.Carrier] = 18,
        [UnitType.Decoy] = 10,
    };

    public UnitDebugRenderer(GraphicsDevice graphicsDevice) // drop mapWidth, mapHeight params
    {
        _icons = new Dictionary<UnitType, Texture2D>();
        foreach (var (type, shape) in ShapeByType)
            _icons[type] = ShapeTextures.Create(graphicsDevice, shape);

        _selectionRingTexture = ShapeTextures.Create(graphicsDevice, IconShape.RingOutline);
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, World world, Entity viewingFaction, bool showAllUnits = false)
    {
        foreach (var (entity, unit, position, ownership) in world.Query<UnitComponent, PositionComponent, OwnershipComponent>())
        {
            bool isOwn = ownership.Owner == viewingFaction;
            bool isDetected = true;

            if (!isOwn)
            {
                var detection = world.Get<DetectionComponent>(entity);
                isDetected = detection != null && detection.DetectedByFactions.Contains(viewingFaction);
                if (!isDetected && !showAllUnits) continue; // fog of war
            }

            var pos = GeoMath.Project(position.Latitude, position.Longitude);
            var texture = _icons[unit.Type];
            int displaySize = SizeByType[unit.Type];
            float scale = displaySize / (float)texture.Width;
            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            var color = Color.White;
            if (ownership.Owner is { } faction)
            {
                var factionInfo = world.Get<FactionComponent>(faction);
                if (factionInfo != null) color = factionInfo.Color;
            }

            float alpha = (!isOwn && !isDetected) ? 0.35f : 1f; // debug-only reveal fades out
            var backingColor = isOwn ? Color.Black * 0.6f : Color.Red * 0.6f;

            spriteBatch.Draw(texture, pos, null, backingColor * alpha, 0f, origin, scale * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, pos, null, color * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            if (isOwn && world.Has<EmconComponent>(entity))
            {
                var emconOrigin = new Vector2(_selectionRingTexture.Width / 2f, _selectionRingTexture.Height / 2f);
                float emconScale = 20f / _selectionRingTexture.Width;
                spriteBatch.Draw(_selectionRingTexture, pos, null, Color.Gray, 0f, emconOrigin, emconScale, SpriteEffects.None, 0f);
            }
        }
    }

    public void DrawSelection(SpriteBatch spriteBatch, World world, Entity? selectedUnit)
    {
        if (!selectedUnit.HasValue || !world.IsAlive(selectedUnit.Value)) return;

        var position = world.Get<PositionComponent>(selectedUnit.Value);
        if (position == null) return;

        var pos = GeoMath.Project(position.Latitude, position.Longitude);
        var origin = new Vector2(_selectionRingTexture.Width / 2f, _selectionRingTexture.Height / 2f);
        float scale = 26f / _selectionRingTexture.Width;

        spriteBatch.Draw(_selectionRingTexture, pos, null, Color.Yellow, 0f, origin, scale, SpriteEffects.None, 0f);

        var order = world.Get<MoveOrderComponent>(selectedUnit.Value);
        if (order != null)
        {
            var targetPos = GeoMath.Project(order.TargetLatitude, order.TargetLongitude);
            DrawLine(spriteBatch, pos, targetPos, Color.Yellow * 0.6f, 2f);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var delta = end - start;
        float length = delta.Length();
        float angle = (float)Math.Atan2(delta.Y, delta.X);
        spriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    public void DrawIncomingStrikes(SpriteBatch spriteBatch, World world)
    {
        foreach (var (entity, strike, position) in world.Query<IncomingStrikeComponent, PositionComponent>())
        {
            var pos = GeoMath.Project(position.Latitude, position.Longitude);
            spriteBatch.Draw(_pixel, new Rectangle((int)pos.X - 4, (int)pos.Y - 4, 8, 8), Color.Red);

            var order = world.Get<MoveOrderComponent>(entity);
            if (order != null)
            {
                var targetPos = GeoMath.Project(order.TargetLatitude, order.TargetLongitude);
                DrawLine(spriteBatch, pos, targetPos, Color.Red * 0.7f, 2f);
            }
        }
    }
    public void DrawRadarCones(SpriteBatch spriteBatch, World world, Entity viewingFaction)
    {
        foreach (var (entity, sensors, position, ownership) in world.Query<SensorsComponent, PositionComponent, OwnershipComponent>())
        {
            if (ownership.Owner != viewingFaction) continue; // only shows your own view. 

            foreach (var sensor in sensors.Sensors)
            {
                if (sensor.Type != SensorType.Radar || sensor.FieldOfViewDegrees >= 360) continue;

                var center = GeoMath.Project(position.Latitude, position.Longitude);
                double halfFov = sensor.FieldOfViewDegrees / 2.0;
                const int arcSegments = 16;

                Vector2 PointAtBearing(double bearingDeg)
                {
                    var (lat, lon) = GeoMath.DestinationPoint(position.Latitude, position.Longitude, bearingDeg * Math.PI / 180.0, sensor.DetectionRadiusKm);
                    return GeoMath.Project(lat, lon);
                }

                var leftEdge = PointAtBearing(sensor.FacingDegrees - halfFov);
                var rightEdge = PointAtBearing(sensor.FacingDegrees + halfFov);
                var coneColor = Color.LimeGreen * 0.35f;

                DrawLine(spriteBatch, center, leftEdge, coneColor, 1.5f);
                DrawLine(spriteBatch, center, rightEdge, coneColor, 1.5f);

                var previous = leftEdge;
                for (int i = 1; i <= arcSegments; i++)
                {
                    double t = (double)i / arcSegments;
                    double bearing = sensor.FacingDegrees - halfFov + sensor.FieldOfViewDegrees * t;
                    var point = PointAtBearing(bearing);
                    DrawLine(spriteBatch, previous, point, coneColor, 1.5f);
                    previous = point;
                }
            }
        }
    }

    public void DrawJammingRadius(SpriteBatch spriteBatch, World world, Entity viewingFaction)
    {
        foreach (var (entity, jammer, position, ownership) in world.Query<JammerComponent, PositionComponent, OwnershipComponent>())
        {
            if (ownership.Owner != viewingFaction || !jammer.IsActive) continue;

            const int segments = 24;
            Vector2 previous = default;

            for (int i = 0; i <= segments; i++)
            {
                double bearing = 360.0 / segments * i;
                var (lat, lon) = GeoMath.DestinationPoint(position.Latitude, position.Longitude, bearing * Math.PI / 180.0, jammer.JamRangeKm);
                var point = GeoMath.Project(lat, lon);

                if (i > 0)
                    DrawLine(spriteBatch, previous, point, Color.Magenta * 0.35f, 1.5f);

                previous = point;
            }
        }
    }
}