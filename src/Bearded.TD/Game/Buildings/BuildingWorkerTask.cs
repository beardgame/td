using System.Collections.Generic;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Resources;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    class BuildingWorkerTask : WorkerTask, IResourceConsumer
    {
        private readonly BuildingBlueprint blueprint;

        private BuildingPlaceholder placeholder;
        private Building building;

        private double resourcesConsumed;
        private int healthGiven = 1;
        private bool finished;

        public override IEnumerable<Tile<TileInfo>> Tiles => building?.OccupiedTiles ?? placeholder?.OccupiedTiles;
        public override bool Finished => finished;

        public BuildingWorkerTask(BuildingPlaceholder placeholder)
        {
            this.placeholder = placeholder;
            blueprint = placeholder.Blueprint;
        }

        public void SetBuilding(Building building)
        {
            DebugAssert.State.Satisfies(this.building == null, "Can only set building once.");
            this.building = building;
            building.Completing += onBuildingCompleting;
        }

        private void onBuildingCompleting()
        {
            // Building is going to be set to be completed. We may not have finished due to network lag.
            // Make sure to apply remaining progress before it is no longer possible.
            // Note that we can assume building is set, because the StartBuildingConstruction command has been called.

            var healthRemaining = blueprint.MaxHealth - healthGiven;
            building.SetBuildProgress(1, healthRemaining);
            building.Completing -= onBuildingCompleting;
            finished = true;
        }

        public override void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
        {
            if (placeholder != null)
            {
                onConstructionStart();
            }

            var remaining = blueprint.ResourceCost - resourcesConsumed;
            resourceManager.RegisterConsumer(this, ratePerS, remaining);
        }

        private void onConstructionStart()
        {
            placeholder.Sync(StartBuildingConstruction.Command);
            placeholder = null;
        }

        public void ConsumeResources(ResourceGrant grant)
        {
            resourcesConsumed += grant.Amount;
            if (building != null)
            {
                updateBuildingToMatch();
            }
            if (grant.ReachedCapacity)
            {
                finished = true;
                building?.Sync(FinishBuildingConstruction.Command);
            }
        }

        private void updateBuildingToMatch()
        {
            // Building was set to completed, probably because of command from server.
            // We want to keep consuming resources though to make sure the resources stay in sync.
            if (building.IsCompleted) return;

            var buildProgress = resourcesConsumed / blueprint.ResourceCost;
            DebugAssert.State.Satisfies(buildProgress <= 1);
            var expectedHealthGiven = Mathf.CeilToInt(buildProgress * blueprint.MaxHealth);
            var newHealthGiven = expectedHealthGiven - healthGiven;
            building.SetBuildProgress(buildProgress, newHealthGiven);
        }
    }
}
