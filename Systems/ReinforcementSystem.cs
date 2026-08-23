using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

public class ReinforcementSystem : ISystem
{
    public void Update(World world, GameTime gameTime)
    {
        double dtHours = gameTime.ElapsedGameTime.TotalHours * SimulationClock.TimeScale;

        foreach (var (_, budget) in world.Query<PlacementBudgetComponent>())
        {
            budget.Points += budget.RegenPerHour * dtHours;
        }
    }
}