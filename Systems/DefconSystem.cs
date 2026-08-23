using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class DefconSystem : ISystem
{
    private const double BaseSecondsPerLevel = 75; // pace at zero tension
    private const double MinSecondsPerLevel = 15;  // fastest possible pace at max tension
    private const double TensionDecayPerSecond = 0.5;

    private readonly Entity _tensionEntity;

    public DefconSystem(Entity tensionEntity)
    {
        _tensionEntity = tensionEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        var tension = world.Get<TensionComponent>(_tensionEntity);
        double tensionValue = tension?.Tension ?? 0;

        if (tension != null)
            tension.Tension = System.Math.Max(0, tension.Tension - gameTime.ElapsedGameTime.TotalSeconds * TensionDecayPerSecond);

        double secondsPerLevel = System.Math.Max(MinSecondsPerLevel, BaseSecondsPerLevel - tensionValue * 0.6);

        foreach (var (_, defcon) in world.Query<DefconComponent>())
        {
            if (defcon.Level <= 1) continue;

            defcon.ProgressToNextLevel += gameTime.ElapsedGameTime.TotalSeconds / secondsPerLevel;
            if (defcon.ProgressToNextLevel >= 1.0)
            {
                defcon.ProgressToNextLevel = 0;
                defcon.Level--;
            }
        }
    }
}