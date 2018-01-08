using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Resources;
using Bearded.TD.Mods.Models;
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

        public override Position2 Position { get; }
        public override bool Finished => finished;

        public BuildingWorkerTask(BuildingPlaceholder placeholder)
        {
            this.placeholder = placeholder;
            blueprint = placeholder.Blueprint;
            Position = placeholder.Position;
        }

        public void SetBuilding(Building building)
        {
            DebugAssert.State.Satisfies(this.building == null, "Can only set building once.");
            this.building = building;
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
            var buildProgress = resourcesConsumed / blueprint.ResourceCost;
            DebugAssert.State.Satisfies(buildProgress <= 1);
            var expectedHealthGiven = Mathf.CeilToInt(buildProgress * blueprint.MaxHealth);
            var newHealthGiven = expectedHealthGiven - healthGiven;
            building.SetBuildProgress(buildProgress, newHealthGiven);
        }
    }
}
