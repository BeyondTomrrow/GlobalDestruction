using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WorldNMilSim.Map;

namespace WorldNMilSim.Rendering;

public class Camera2D
{
    private readonly GraphicsDevice _graphicsDevice;

    public Vector2 Position; // world-space point the camera is centered on
    public float ZoomLevel;
    public float MinZoom;
    public float MaxZoom;

    // Zoom level at which the map exactly covers the window (shorter edge matches, longer edge is cropped).
    // Also the hard floor for ZoomLevel, so black space around the map is never possible.
    public float FitZoom { get; }

    public Camera2D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        Position = new Vector2(MapSpace.WIDTH / 2f, MapSpace.HEIGHT / 2f);

        var viewport = graphicsDevice.Viewport;
        FitZoom = Math.Max(viewport.Width / (float)MapSpace.WIDTH, viewport.Height / (float)MapSpace.HEIGHT);
        ZoomLevel = FitZoom;
        MinZoom = FitZoom;
        MaxZoom = FitZoom * 15f;
    }

    public Matrix GetViewMatrix()
    {
        var viewport = _graphicsDevice.Viewport;
        return Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
               Matrix.CreateScale(ZoomLevel) *
               Matrix.CreateTranslation(new Vector3(viewport.Width / 2f, viewport.Height / 2f, 0f));
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix()));

    public Vector2 WorldToScreen(Vector2 worldPosition) => Vector2.Transform(worldPosition, GetViewMatrix());

    public void ZoomAt(float multiplier, Vector2 screenFocusPoint)
    {
        var worldBefore = ScreenToWorld(screenFocusPoint);
        ZoomLevel = MathHelper.Clamp(ZoomLevel * multiplier, MinZoom, MaxZoom);
        var worldAfter = ScreenToWorld(screenFocusPoint);
        Position += worldBefore - worldAfter;
        ClampToMapBounds();
    }

    public void Pan(Vector2 worldDelta)
    {
        Position += worldDelta;
        ClampToMapBounds();
    }

    private void ClampToMapBounds()
    {
        var viewport = _graphicsDevice.Viewport;
        float halfWidthWorld = viewport.Width / 2f / ZoomLevel;
        float halfHeightWorld = viewport.Height / 2f / ZoomLevel;

        float minX = halfWidthWorld;
        float maxX = MapSpace.WIDTH - halfWidthWorld;
        float minY = halfHeightWorld;
        float maxY = MapSpace.HEIGHT - halfHeightWorld;

        // If the map is narrower than the viewport on an axis (only possible right at FitZoom on
        // the "matched" axis), there's no room to pan on it - lock to center instead of clamping.
        Position.X = minX <= maxX ? MathHelper.Clamp(Position.X, minX, maxX) : MapSpace.WIDTH / 2f;
        Position.Y = minY <= maxY ? MathHelper.Clamp(Position.Y, minY, maxY) : MapSpace.HEIGHT / 2f;
    }
}