using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System.Collections.Generic;

namespace WorldNMilSim.Rendering;

public class UnitDebugRenderer
{
    private readonly Dictionary<UnitType, Texture2D> _icons;
    private readonly int _mapWidth;
    private readonly int _mapHeight;

    private static readonly Dictionary<UnitType, IconShape> ShapeByType = new()
    {
        [UnitType.Silo] = IconShape.Diamond,
        [UnitType.RadarStation] = IconShape.RingOutline,
        [UnitType.Airbase] = IconShape.Triangle,
        [UnitType.Destroyer] = IconShape.Square,
        [UnitType.Submarine] = IconShape.Circle,
        [UnitType.Carrier] = IconShape.Square,
    };

    private static readonly Dictionary<UnitType, int> SizeByType = new()
    {
        [UnitType.Silo] = 14,
        [UnitType.RadarStation] = 18,
        [UnitType.Airbase] = 14,
        [UnitType.Destroyer] = 10,
        [UnitType.Submarine] = 10,
        [UnitType.Carrier] = 18,
    };

    public UnitDebugRenderer(GraphicsDevice graphicsDevice, int mapWidth, int mapHeight)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;

        _icons = new Dictionary<UnitType, Texture2D>();
        foreach (var (type, shape) in ShapeByType)
            _icons[type] = ShapeTextures.Create(graphicsDevice, shape);
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

            var pos = GeoMath.Project(position.Latitude, position.Longitude, _mapWidth, _mapHeight);
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
        }
    }
}