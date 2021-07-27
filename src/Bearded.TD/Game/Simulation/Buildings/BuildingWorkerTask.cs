using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingWorkerTask : IWorkerTask
    {
        // private readonly Building building;
        private readonly IBuildingConstructor buildingConstructor;
        private readonly ResourceConsumer resourceConsumer;

        private bool started;
        private HitPoints healthGiven = new(1);
        private readonly HitPoints maxHealth;

        public Id<IWorkerTask> Id { get; }
        public string Name => $"Build {buildingConstructor.StructureName}";
        public IEnumerable<Tile> Tiles { get; }
        public double PercentCompleted => !started ? 0 : healthGiven / maxHealth;
        public bool CanAbort => !started;
        public bool Finished { get; private set; }

        public BuildingWorkerTask(
            Id<IWorkerTask> taskId,
            GameState gameState,
            IBuildingConstructor buildingConstructor,
            IEnumerable<Tile> tiles,
            HitPoints maxHealth,
            FactionResources.IResourceReservation resourceReservation)
        {
            Id = taskId;
            this.buildingConstructor = buildingConstructor;
            Tiles = tiles;
            this.maxHealth = maxHealth;
            resourceConsumer = new ResourceConsumer(gameState, resourceReservation, 0.ResourcesPerSecond());
            buildingConstructor.Completing += onConstructionCompleting;
        }

        private void onConstructionCompleting()
        {
            // Building is going to be set to be completed. We may not have finished due to network lag.
            // Make sure to apply remaining progress before it is no longer possible.
            // Note that we can assume building is set, because the StartBuildingConstruction command has been called.

            resourceConsumer.CompleteIfNeeded();
            var healthRemaining = maxHealth - healthGiven;
            buildingConstructor.ProgressBuild(healthRemaining);
            buildingConstructor.Completing -= onConstructionCompleting;
            Finished = true;
        }

        public void Progress(TimeSpan elapsedTime, IWorkerParameters workerParameters)
        {
            if (!started && resourceConsumer.CanConsume)
            {
                onConstructionStart();
            }
            if (buildingConstructor.IsCancelled)
            {
                buildingConstructor.Completing -= onConstructionCompleting;
                Finished = true;
                return;
            }

            if (workerParameters.BuildingSpeed != resourceConsumer.ConsumptionRate)
            {
                resourceConsumer.UpdateConsumptionRate(workerParameters.BuildingSpeed);
            }

            resourceConsumer.PrepareIfNeeded();
            resourceConsumer.Update();
            updateBuildingToMatch();
            if (resourceConsumer.IsDone)
            {
                buildingConstructor.CompleteBuild();
            }
        }

        public void OnAbort()
        {
            resourceConsumer.Abort();
            buildingConstructor.AbortBuild();
        }

        private void onConstructionStart()
        {
            started = true;
            buildingConstructor.StartBuild();
        }

        private void updateBuildingToMatch()
        {
            // Building was set to completed, probably because of command from server.
            // We want to keep consuming resources though to make sure the resources stay in sync.
            if (buildingConstructor.IsCompleted) return;

            var buildProgress = resourceConsumer.PercentageDone;
            State.Satisfies(buildProgress <= 1);
            var expectedHealthGiven = new HitPoints(MoreMath.CeilToInt(buildProgress * maxHealth.NumericValue));
            if (expectedHealthGiven == HitPoints.Zero)
            {
                return;
            }
            var newHealthGiven = expectedHealthGiven - healthGiven;
            buildingConstructor.ProgressBuild(newHealthGiven);
            healthGiven = expectedHealthGiven;
        }
    }
}
