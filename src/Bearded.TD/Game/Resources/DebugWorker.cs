using Bearded.TD.Game.Buildings;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    class DebugWorker : GameObject
    {
        private const double buildRate = 5;

        private readonly DebugWorkerResourceConsumer consumer;

        public DebugWorker(Building.BuildProcessManager processManager)
        {
            consumer = new DebugWorkerResourceConsumer(processManager);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (consumer.Finished)
            {
                Delete();
                return;
            }
            Game.Resources.RegisterConsumer(consumer);
        }

        public override void Draw(GeometryManager geometries)
        { }

        private class DebugWorkerResourceConsumer : IResourceConsumer
        {
            private readonly Building.BuildProcessManager processManager;

            public bool Finished => processManager.ResourcesStillNeeded <= 0;

            public DebugWorkerResourceConsumer(Building.BuildProcessManager processManager)
            {
                this.processManager = processManager;
            }

            public double RatePerS => buildRate;
            public double Maximum => processManager.ResourcesStillNeeded;
            public void ConsumeResources(ResourceGrant grant) => processManager.Progress(grant);
        }
    }
}