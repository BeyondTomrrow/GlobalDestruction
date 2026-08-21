using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldNMilSim.Rendering;

public enum IconShape { Square, Circle, RingOutline, Triangle, Diamond }

public static class ShapeTextures
{
    public static Texture2D Create(GraphicsDevice device, IconShape shape, int size = 32)
    {
        var data = new Color[size * size];
        Array.Fill(data, Color.Transparent);

        switch (shape)
        {
            case IconShape.Square:
                for (int i = 0; i < data.Length; i++) data[i] = Color.White;
                break;
            case IconShape.Circle:
                DrawCircle(data, size, filled: true);
                break;
            case IconShape.RingOutline:
                DrawCircle(data, size, filled: false);
                break;
            case IconShape.Triangle:
                DrawTriangle(data, size);
                break;
            case IconShape.Diamond:
                DrawDiamond(data, size);
                break;
        }

        var texture = new Texture2D(device, size, size);
        texture.SetData(data);
        return texture;
    }

    private static void DrawCircle(Color[] data, int size, bool filled)
    {
        float radius = size / 2f;
        var center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
            bool on = filled ? dist <= radius : (dist <= radius && dist >= radius - 3);
            if (on) data[y * size + x] = Color.White;
        }
    }

    private static void DrawTriangle(Color[] data, int size)
    {
        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            float t = y / (float)(size - 1);
            int halfWidth = (int)(t * center);
            for (int x = center - halfWidth; x <= center + halfWidth; x++)
                if (x >= 0 && x < size) data[y * size + x] = Color.White;
        }
    }

    private static void DrawDiamond(Color[] data, int size)
    {
        float half = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Math.Abs(x + 0.5f - half);
            float dy = Math.Abs(y + 0.5f - half);
            if (dx / half + dy / half <= 1f) data[y * size + x] = Color.White;
        }
    }
}