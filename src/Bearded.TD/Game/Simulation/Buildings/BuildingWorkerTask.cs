using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingWorkerTask : IWorkerTask
    {
        private readonly IIncompleteBuilding incompleteBuilding;
        private readonly ResourceConsumer resourceConsumer;

        private bool started;

        public Id<IWorkerTask> Id { get; }
        public string Name => $"Build {incompleteBuilding.StructureName}";
        public IEnumerable<Tile> Tiles { get; }
        public double PercentCompleted => incompleteBuilding.PercentageComplete;
        public bool CanAbort => !started;
        public bool Finished { get; private set; }

        public BuildingWorkerTask(
            Id<IWorkerTask> taskId,
            GameState gameState,
            IIncompleteBuilding incompleteBuilding,
            IEnumerable<Tile> tiles,
            FactionResources.IResourceReservation resourceReservation)
        {
            Id = taskId;
            this.incompleteBuilding = incompleteBuilding;
            Tiles = tiles;
            // The resources per second gets overwritten each frame to account for worker speed changes.
            resourceConsumer = new ResourceConsumer(gameState, resourceReservation, 0.ResourcesPerSecond());
        }

        public void Progress(TimeSpan elapsedTime, IWorkerParameters workerParameters)
        {
            if (incompleteBuilding.IsCompleted)
            {
                resourceConsumer.CompleteIfNeeded();
            }
            if (incompleteBuilding.IsCompleted || incompleteBuilding.IsCancelled)
            {
                Finished = true;
                return;
            }

            if (workerParameters.BuildingSpeed != resourceConsumer.ConsumptionRate)
            {
                resourceConsumer.UpdateConsumptionRate(workerParameters.BuildingSpeed);
            }

            resourceConsumer.PrepareIfNeeded();
            if (!resourceConsumer.CanConsume)
            {
                return;
            }

            if (!started)
            {
                onConstructionStart();
            }

            resourceConsumer.Update();
            updateBuildingToMatch();
            if (resourceConsumer.IsDone)
            {
                incompleteBuilding.CompleteBuild();
            }
        }

        public void OnAbort()
        {
            resourceConsumer.Abort();
            incompleteBuilding.CancelBuild();
        }

        private void onConstructionStart()
        {
            started = true;
            incompleteBuilding.StartBuild();
        }

        private void updateBuildingToMatch()
        {
            // Building was set to completed, probably because of command from server.
            // We want to keep consuming resources though to make sure the resources stay in sync.
            if (incompleteBuilding.IsCompleted) return;

            incompleteBuilding.SetBuildProgress(resourceConsumer.PercentageDone);
        }
    }
}
