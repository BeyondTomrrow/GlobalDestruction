using WorldNMilSim.Core;
using WorldNMilSim.Components;

namespace WorldNMilSim.Systems;

// Applies population loss to a city and records it against both factions involved.
// Event-driven (called by whatever inflicts the damage - nuclear strikes, later maybe others),
// not a per-tick ISystem.
public static class CasualtyTracker
{
    public static int Apply(World world, Entity city, int deaths, Entity attackerFaction)
    {
        var population = world.Get<PopulationComponent>(city);
        if (population == null) return 0;

        int actualDeaths = System.Math.Min(deaths, population.CurrentPopulation);
        population.CurrentPopulation -= actualDeaths;

        var attackerInfo = world.Get<FactionComponent>(attackerFaction);
        if (attackerInfo != null)
            attackerInfo.TotalCasualtiesInflicted += actualDeaths;

        var cityInfo = world.Get<CityComponent>(city);
        if (cityInfo != null)
        {
            var ownership = world.Get<OwnershipComponent>(cityInfo.ParentTerritory);
            if (ownership?.Owner is { } ownerFaction)
            {
                var ownerInfo = world.Get<FactionComponent>(ownerFaction);
                if (ownerInfo != null)
                    ownerInfo.TotalCasualtiesSuffered += actualDeaths;
            }
        }

        return actualDeaths;
    }
}