using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class ChatterLogSystem : ISystem
{
    private readonly Entity _chatterEntity;

    public ChatterLogSystem(Entity chatterEntity)
    {
        _chatterEntity = chatterEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        var log = world.Get<ChatterLogComponent>(_chatterEntity);
        if (log == null) return;

        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        for (int i = log.Messages.Count - 1; i >= 0; i--)
        {
            log.Messages[i].RemainingSeconds -= dtSeconds;
            if (log.Messages[i].RemainingSeconds <= 0)
                log.Messages.RemoveAt(i);
        }
    }
}