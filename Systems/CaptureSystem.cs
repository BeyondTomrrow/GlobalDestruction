using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WorldNMilSim.Core;
using WorldNMilSim.Components;
using WorldNMilSim.Map;

namespace WorldNMilSim.Systems;

public class CaptureSystem : ISystem
{
    private const double CaptureRadiusKm = 250;
    private const double CaptureRateFractionPerSecond = 1.0 / 30.0; // ~30s of uncontested presence to flip

    private readonly Entity _chatterEntity;

    public CaptureSystem(Entity chatterEntity)
    {
        _chatterEntity = chatterEntity;
    }

    public void Update(World world, GameTime gameTime)
    {
        double dtSeconds = gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var (territoryEntity, territory, ownership, captureState) in world.Query<TerritoryComponent, OwnershipComponent, CaptureStateComponent>())
        {
            var presentFactions = new HashSet<Entity>();
            foreach (var (unitEntity, unitInfo, position, unitOwnership) in world.Query<UnitComponent, PositionComponent, OwnershipComponent>())
            {
                if (unitInfo.Type != UnitType.Army || unitOwnership.Owner is not { } unitFaction) continue;

                double distanceKm = GeoMath.HaversineDistanceKm(territory.Latitude, territory.Longitude, position.Latitude, position.Longitude);
                if (distanceKm <= CaptureRadiusKm)
                    presentFactions.Add(unitFaction);
            }

            if (presentFactions.Count != 1)
            {
                // Empty or contested by multiple factions - no progress, existing progress decays.
                captureState.Progress = System.Math.Max(0, captureState.Progress - dtSeconds * CaptureRateFractionPerSecond);
                captureState.CapturingFaction = null;
                continue;
            }

            Entity presentFaction = presentFactions.First();

            if (ownership.Owner == presentFaction)
            {
                // Owner's own troops are home defending - nothing to capture.
                captureState.Progress = 0;
                captureState.CapturingFaction = null;
                continue;
            }

            if (captureState.CapturingFaction != presentFaction)
            {
                captureState.Progress = 0;
                captureState.CapturingFaction = presentFaction;
            }

            captureState.Progress += dtSeconds * CaptureRateFractionPerSecond;

            if (captureState.Progress >= 1.0)
            {
                ownership.Owner = presentFaction;

                var factionInfo = world.Get<FactionComponent>(presentFaction);
                ChatterLog.Post(world, _chatterEntity, $"{factionInfo?.Name ?? "Unknown"} forces have taken {territory.Name}");
                captureState.Progress = 0;
                captureState.CapturingFaction = null;
            }
        }
    }
}