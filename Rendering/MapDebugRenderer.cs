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

    private const double MajorCityPopulationThreshold = 45;

    public MapDebugRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

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
            Color color;

            if (territory.Kind == TerritoryKind.Land)
            {
                var ownership = world.Get<OwnershipComponent>(entity);
                var factionInfo = ownership?.Owner is { } owner ? world.Get<FactionComponent>(owner) : null;
                color = factionInfo?.Color ?? Color.ForestGreen;
            }
            else
            {
                color = Color.DeepSkyBlue;
            }

            int size = territory.Kind == TerritoryKind.Land ? 10 : 6;
            spriteBatch.Draw(_pixel, new Rectangle((int)pos.X - size / 2, (int)pos.Y - size / 2, size, size), color);
        }

        foreach (var (entity, city, position) in world.Query<CityComponent, PositionComponent>())
        {
            if (city.IsCapital) continue;

            var pos = GeoMath.Project(position.Latitude, position.Longitude);
            spriteBatch.Draw(_pixel, new Rectangle((int)pos.X - 3, (int)pos.Y - 3, 6, 6), Color.LightGray);
        }

        foreach (var (entity, territory, captureState) in world.Query<TerritoryComponent, CaptureStateComponent>())
        {
            if (captureState.CapturingFaction is not { } capturingFaction || captureState.Progress <= 0) continue;

            var factionInfo = world.Get<FactionComponent>(capturingFaction);
            if (factionInfo == null) continue;

            var pos = GeoMath.Project(territory.Latitude, territory.Longitude);
            const int barWidth = 24;
            const int barHeight = 4;
            int filledWidth = (int)(barWidth * captureState.Progress);

            var barPos = new Rectangle((int)pos.X - barWidth / 2, (int)pos.Y + 10, barWidth, barHeight);
            spriteBatch.Draw(_pixel, barPos, Color.Black * 0.6f);
            spriteBatch.Draw(_pixel, new Rectangle(barPos.X, barPos.Y, filledWidth, barHeight), factionInfo.Color);
        }
    }


    public void DrawLabels(SpriteBatch spriteBatch, World world, Camera2D camera, SpriteFont font)
    {
        bool zoomedIn = camera.ZoomLevel > camera.FitZoom * 2f;

        foreach (var (entity, city, position, population) in world.Query<CityComponent, PositionComponent, PopulationComponent>())
        {
            bool isMajor = population.MaxPopulation >= MajorCityPopulationThreshold;
            if (!zoomedIn && !isMajor) continue;

            var screenPos = camera.WorldToScreen(GeoMath.Project(position.Latitude, position.Longitude));
            var textPos = screenPos + new Vector2(10, -font.LineSpacing / 2f);

            string labelText = $"{city.Name} ({population.CurrentPopulation})";

            spriteBatch.DrawString(font, labelText, textPos + Vector2.One, Color.Black * 0.7f);
            spriteBatch.DrawString(font, labelText, textPos, Color.White);
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