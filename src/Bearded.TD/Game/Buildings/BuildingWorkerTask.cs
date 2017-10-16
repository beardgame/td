using Bearded.TD.Game.Resources;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Buildings
{
    abstract partial class Building
    {
        private class BuildingWorkerTask : WorkerTask, IResourceConsumer
        {
            private readonly Building building;
            private readonly BuildingBlueprint blueprint;
            public override Position2 Position => building.Position;

            private double buildProgress;
            private int healthGiven = 1;
            private bool finished;
            public override bool Finished => finished;

            private double currentProgressFraction => buildProgress / blueprint.ResourceCost;

            public BuildingWorkerTask(Building building, BuildingBlueprint blueprint)
            {
                this.building = building;
                this.blueprint = blueprint;
                building.Deleting += onBuildingAborted;
            }

            public override void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, double ratePerS)
            {
                var remaining = blueprint.ResourceCost - buildProgress;
                if (remaining <= 0)
                {
                    completeBuilding();
                    return;
                }
                resourceManager.RegisterConsumer(this, ratePerS, remaining);
            }

            public void ConsumeResources(ResourceGrant resources)
            {
                if (buildProgress >= blueprint.ResourceCost || building.Deleted) return;

                if (resources.ReachedCapacity)
                {
                    completeBuilding();
                    return;
                }

                buildProgress += resources.Amount;
                var expectedHealthGiven = (int)(currentProgressFraction * blueprint.MaxHealth);
                if (expectedHealthGiven < healthGiven)
                    return;
                building.Health += expectedHealthGiven - healthGiven;
                building.buildProgress = buildProgress / blueprint.MaxHealth;
                healthGiven = expectedHealthGiven;
            }

            private void completeBuilding()
            {
                buildProgress = blueprint.ResourceCost;
                building.Health += blueprint.MaxHealth - healthGiven;
                building.onCompleted();
                finished = true;
                building.Deleting -= onBuildingAborted;
            }

            private void onBuildingAborted() => finished = true;
        }
    }
}
