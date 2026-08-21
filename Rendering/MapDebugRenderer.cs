using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System.Collections.Generic;
using System;

namespace WorldNMilSim.Rendering;

// Quick visual sanity-check for the map data - dots for territories, lines for routes.
// Replace with real map art/UI later; this just proves the graph is wired correctly.
public class MapDebugRenderer
{
    private readonly Texture2D _pixel;
    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public MapDebugRenderer(GraphicsDevice graphicsDevice, int mapWidth, int mapHeight)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    public void Draw(SpriteBatch spriteBatch, World world)
    {
        // Routes first, so dots draw on top of lines.
        var drawnPairs = new HashSet<(int, int)>();
        foreach (var (entity, territory, adjacency) in world.Query<TerritoryComponent, AdjacencyComponent>())
        {
            var from = GeoMath.Project(territory.Latitude, territory.Longitude, _mapWidth, _mapHeight);
            foreach (var route in adjacency.Routes)
            {
                var key = (Math.Min(entity.Id, route.Target.Id), Math.Max(entity.Id, route.Target.Id));
                if (!drawnPairs.Add(key)) continue;

                var targetTerritory = world.Get<TerritoryComponent>(route.Target)!;
                var to = GeoMath.Project(targetTerritory.Latitude, targetTerritory.Longitude, _mapWidth, _mapHeight);

                var color = route.Kind == RouteKind.Land ? Color.SaddleBrown : Color.SteelBlue;
                DrawLine(spriteBatch, from, to, color * 0.5f, 1);
            }
        }

        foreach (var (entity, territory) in world.Query<TerritoryComponent>())
        {
            var pos = GeoMath.Project(territory.Latitude, territory.Longitude, _mapWidth, _mapHeight);
            var color = territory.Kind == TerritoryKind.Land ? Color.ForestGreen : Color.DeepSkyBlue;
            int size = territory.Kind == TerritoryKind.Land ? 10 : 6;
            spriteBatch.Draw(_pixel, new Rectangle((int)pos.X - size / 2, (int)pos.Y - size / 2, size, size), color);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
    {
        var delta = end - start;
        float length = delta.Length();
        float angle = (float)Math.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(
            _pixel,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }
}