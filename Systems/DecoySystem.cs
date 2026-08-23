using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class DecoySystem : ISystem
{
    public void Update(World world, GameTime gameTime)
    {
        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds; // real time - decoys are short-lived, should feel real-time

        foreach (var (_, launcher) in world.Query<DecoyLauncherComponent>())
        {
            if (launcher.CooldownRemaining > 0)
                launcher.CooldownRemaining -= dtSeconds;
        }

        var expired = new List<Entity>();
        foreach (var (entity, decoy) in world.Query<DecoyComponent>())
        {
            decoy.RemainingSeconds -= dtSeconds;
            if (decoy.RemainingSeconds <= 0)
                expired.Add(entity);
        }

        foreach (var entity in expired)
            world.DestroyEntity(entity);
    }
}