using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;
using System;
using System.Collections.Generic;

namespace WorldNMilSim.Rendering;

public class MapDebugRenderer
{
    private readonly Texture2D _pixel;

    private const double MajorCityPopulationThreshold = 80;

    public MapDebugRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    // World-space pass: dots and route lines. Draw inside a SpriteBatch using the camera's transform matrix.
    public void Draw(SpriteBatch spriteBatch, World world)
    {
        var drawnPairs = new HashSet<(int, int)>();
        foreach (var (entity, territory, adjacency) in world.Query<TerritoryComponent, AdjacencyComponent>())
        {
            var from = GeoMath.Project(territory.Latitude, territory.Longitude);
            foreach (var route in adjacency.Routes)
            {
                var key = (Math.Min(entity.Id, route.Target.Id), Math.Max(entity.Id, route.Target.Id));
                if (!drawnPairs.Add(key)) continue;

                var targetTerritory = world.Get<TerritoryComponent>(route.Target)!;
                var to = GeoMath.Project(targetTerritory.Latitude, targetTerritory.Longitude);

                var color = route.Kind == RouteKind.Land ? Color.SaddleBrown : Color.SteelBlue;
                DrawLine(spriteBatch, from, to, color * 0.5f, 2f);
            }
        }

        foreach (var (entity, territory) in world.Query<TerritoryComponent>())
        {
            var pos = GeoMath.Project(territory.Latitude, territory.Longitude);
            var color = territory.Kind == TerritoryKind.Land ? Color.ForestGreen : Color.DeepSkyBlue;
            int size = territory.Kind == TerritoryKind.Land ? 10 : 6;
            spriteBatch.Draw(_pixel, new Rectangle((int)pos.X - size / 2, (int)pos.Y - size / 2, size, size), color);
        }
    }

    // Screen-space pass: text labels at a constant on-screen size regardless of zoom.
    // Draw this in a SEPARATE SpriteBatch.Begin() with no transform matrix.
    public void DrawLabels(SpriteBatch spriteBatch, World world, Camera2D camera, SpriteFont font)
    {
        bool zoomedIn = camera.ZoomLevel > camera.FitZoom * 2f;

        foreach (var (entity, territory) in world.Query<TerritoryComponent>())
        {
            if (territory.Kind != TerritoryKind.Land) continue;

            var population = world.Get<PopulationComponent>(entity);
            bool isMajor = population != null && population.MaxPopulation >= MajorCityPopulationThreshold;

            if (!zoomedIn && !isMajor) continue; // zoomed out: only label major cities

            var screenPos = camera.WorldToScreen(GeoMath.Project(territory.Latitude, territory.Longitude));
            var textPos = screenPos + new Vector2(10, -font.LineSpacing / 2f);

            spriteBatch.DrawString(font, territory.Name, textPos + Vector2.One, Color.Black * 0.7f);
            spriteBatch.DrawString(font, territory.Name, textPos, Color.White);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var delta = end - start;
        float length = delta.Length();
        float angle = (float)Math.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
}