using Microsoft.Xna.Framework;

namespace WorldNMilSim.Core;

public class SystemManager
{
    private readonly System.Collections.Generic.List<ISystem> _systems = new();

    public SystemManager Add(ISystem system)
    {
        _systems.Add(system);
        return this;
    }

    public void Update(World world, GameTime gameTime)
    {
        foreach (var system in _systems)
            system.Update(world, gameTime);
    }
}