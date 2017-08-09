using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    class DebugWorker : GameObject
    {
        private const double buildRate = 5;

        private readonly Faction faction;
        private readonly DebugWorkerResourceConsumer consumer;

        public DebugWorker(Faction faction, Building.BuildProcessManager processManager)
        {
            this.faction = faction;
            consumer = new DebugWorkerResourceConsumer(processManager);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (consumer.Finished)
            {
                Delete();
                return;
            }
            faction.Resources.RegisterConsumer(consumer);
        }

        public override void Draw(GeometryManager geometries)
        { }

        private class DebugWorkerResourceConsumer : IResourceConsumer
        {
            private readonly Building.BuildProcessManager processManager;

            public bool Finished { get; private set; }

            public DebugWorkerResourceConsumer(Building.BuildProcessManager processManager)
            {
                this.processManager = processManager;

                processManager.Completed += () => Finished = true;
                processManager.Aborted += () => Finished = true;
            }

            public double RatePerS => buildRate;
            public double Maximum => processManager.ResourcesStillNeeded;
            public void ConsumeResources(ResourceGrant grant) => processManager.Progress(grant);
        }
    }
}