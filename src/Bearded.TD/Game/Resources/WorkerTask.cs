using Bearded.TD.Game.Buildings;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    abstract class WorkerTask
    {
        public static WorkerTask ForBuilding(Building building)
            => new BuildingWorkerTask(building);

        public abstract Position2 Position { get; }
        public abstract bool Finished { get; }

        public abstract void Progress(ResourceManager resourceManager, double ratePerS);

        private class BuildingWorkerTask : WorkerTask, IResourceConsumer
        {
            private readonly Building.BuildProcessManager processManager;
            public override Position2 Position { get; }

            private bool finished;
            public override bool Finished => finished;

            public double RatePerS { get; private set; }

            public BuildingWorkerTask(Building building)
            {
                processManager = building.BuildManager;
                Position = building.Position;

                processManager.Completed += () => finished = true;
                processManager.Aborted += () => finished = true;
            }

            public override void Progress(ResourceManager resourceManager, double ratePerS)
            {
                RatePerS = ratePerS;
                resourceManager.RegisterConsumer(this);
            }
                
            public double Maximum => processManager.ResourcesStillNeeded;
            public void ConsumeResources(ResourceGrant grant) => processManager.Progress(grant);
        }
    }
}
