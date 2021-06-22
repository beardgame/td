using System.Collections.Generic;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingWorkerTask : IWorkerTask
    {
        private readonly Building building;
        private readonly ResourceConsumer resourceConsumer;

        private bool started;
        private HitPoints healthGiven = new(1);
        private HitPoints maxHealth = new(1);

        public Id<IWorkerTask> Id { get; }
        public string Name => $"Build {building.Blueprint.Name}";
        public IEnumerable<Tile> Tiles => building.OccupiedTiles;
        public Maybe<ISelectable> Selectable => Maybe.Just<ISelectable>(building);
        public double PercentCompleted => !started ? 0 : healthGiven / maxHealth;
        public bool CanAbort => !started;
        public bool Finished { get; private set; }

        public BuildingWorkerTask(
            Id<IWorkerTask> taskId,
            Building building,
            FactionResources.IResourceReservation resourceReservation)
        {
            Id = taskId;
            this.building = building;
            resourceConsumer = new ResourceConsumer(building.Game, resourceReservation, 0.ResourcesPerSecond());

            building.Completing += onBuildingCompleting;
            building.GetComponents<Health<Building>>()
                .MaybeSingle()
                .Match(health => maxHealth = health.MaxHealth);
        }

        private void onBuildingCompleting()
        {
            // Building is going to be set to be completed. We may not have finished due to network lag.
            // Make sure to apply remaining progress before it is no longer possible.
            // Note that we can assume building is set, because the StartBuildingConstruction command has been called.

            resourceConsumer.CompleteIfNeeded();
            var healthRemaining = maxHealth - healthGiven;
            building!.SetBuildProgress(healthRemaining);
            building.Completing -= onBuildingCompleting;
            Finished = true;
        }

        public void Progress(TimeSpan elapsedTime, IWorkerParameters workerParameters)
        {
            if (!started && resourceConsumer.CanConsume)
            {
                onConstructionStart();
            }
            if (building.Deleted)
            {
                building.Completing -= onBuildingCompleting;
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
                building.Sync(FinishBuildingConstruction.Command);
            }
        }

        public void OnAbort()
        {
            resourceConsumer.Abort();
            building.Delete();
        }

        private void onConstructionStart()
        {
            started = true;
            building.Sync(StartBuildingConstruction.Command);
        }

        private void updateBuildingToMatch()
        {
            // Building was set to completed, probably because of command from server.
            // We want to keep consuming resources though to make sure the resources stay in sync.
            if (building!.IsBuildCompleted) return;

            var buildProgress = resourceConsumer.PercentageDone;
            State.Satisfies(buildProgress <= 1);
            var expectedHealthGiven = new HitPoints(MoreMath.CeilToInt(buildProgress * maxHealth.NumericValue));
            if (expectedHealthGiven == HitPoints.Zero)
            {
                return;
            }
            var newHealthGiven = expectedHealthGiven - healthGiven;
            building.SetBuildProgress(newHealthGiven);
            healthGiven = expectedHealthGiven;
        }
    }
}
