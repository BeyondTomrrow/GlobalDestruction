using System;
using Microsoft.Xna.Framework.Graphics;

namespace WorldNMilSim.Map;

// Samples the world map texture once at load time into a low-res land/sea grid,
// so movement and placement can respect real coastlines instead of the abstract territory graph.
public class TerrainMap
{
    private readonly bool[,] _isSea;
    private readonly int _gridWidth;
    private readonly int _gridHeight;

    public TerrainMap(Texture2D worldMapTexture, int gridWidth = 720, int gridHeight = 360)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _isSea = new bool[gridWidth, gridHeight];

        var pixels = new Microsoft.Xna.Framework.Color[worldMapTexture.Width * worldMapTexture.Height];
        worldMapTexture.GetData(pixels);

        const int blueMargin = 5; // require blue to clearly dominate, so snow/ice (roughly equal RGB) reads as land

        for (int gy = 0; gy < gridHeight; gy++)
        {
            for (int gx = 0; gx < gridWidth; gx++)
            {
                int px = gx * worldMapTexture.Width / gridWidth;
                int py = gy * worldMapTexture.Height / gridHeight;
                var pixel = pixels[py * worldMapTexture.Width + px];

                _isSea[gx, gy] = pixel.B > pixel.R + blueMargin && pixel.B > pixel.G + blueMargin;
            }
        }
    }

    public bool IsSea(double latitude, double longitude)
    {
        int gx = (int)((longitude + 180.0) / 360.0 * _gridWidth);
        int gy = (int)((90.0 - latitude) / 180.0 * _gridHeight);
        gx = Math.Clamp(gx, 0, _gridWidth - 1);
        gy = Math.Clamp(gy, 0, _gridHeight - 1);
        return _isSea[gx, gy];
    }

    public bool IsLand(double latitude, double longitude) => !IsSea(latitude, longitude);
}