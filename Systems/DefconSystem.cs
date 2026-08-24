using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class DefconSystem : ISystem
{
    private const double SecondsForFullDescent = 15; // time per level at maximum tension (100)
    private const double TensionDecayPerSecond = 0.5;

    private readonly Entity _tensionEntity;
    private readonly Entity _chatterEntity;

    public DefconSystem(Entity tensionEntity, Entity chatterEntity)
    {
        _tensionEntity = tensionEntity;
        _chatterEntity = chatterEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        var tension = world.Get<TensionComponent>(_tensionEntity);
        double tensionValue = tension?.Tension ?? 0;

        if (tension != null)
            tension.Tension = System.Math.Max(0, tension.Tension - gameTime.ElapsedGameTime.TotalSeconds * TensionDecayPerSecond);

        double progressPerSecond = tensionValue / 100.0 / SecondsForFullDescent;
        if (progressPerSecond <= 0) return; // zero aggression - DEFCON stays put indefinitely

        foreach (var (_, defcon) in world.Query<DefconComponent>())
        {
            if (defcon.Level <= 1) continue;

            defcon.ProgressToNextLevel += gameTime.ElapsedGameTime.TotalSeconds * progressPerSecond;
            if (defcon.ProgressToNextLevel >= 1.0)
            {
                
                defcon.ProgressToNextLevel = 0;
                defcon.Level--;
                ChatterLog.Post(world, _chatterEntity, $"DEFCON status changed to {defcon.Level}");
            }
        }
    }
}