using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class DefconSystem : ISystem
{
    private const double TotalDescentSeconds = 300; // 5 real minutes DEFCON 5 -> 1
    private const int LevelsToDescend = 4;

    public void Update(World world, GameTime gameTime)
    {
        foreach (var (_, defcon) in world.Query<DefconComponent>())
        {
            if (defcon.Level <= 1) continue;

            defcon.ElapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

            double secondsPerLevel = TotalDescentSeconds / LevelsToDescend;
            int levelsPassed = (int)(defcon.ElapsedSeconds / secondsPerLevel);
            defcon.Level = System.Math.Max(1, 5 - levelsPassed);
        }
    }
}