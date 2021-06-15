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
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingWorkerTask : IWorkerTask
    {
        private readonly IBuildingBlueprint blueprint;
        private readonly ResourceConsumer resourceConsumer;

        private BuildingPlaceholder? placeholder;
        private Building? building;

        private HitPoints healthGiven = new(1);
        private HitPoints maxHealth = new(1);

        public Id<IWorkerTask> Id { get; }
        public string Name => $"Build {blueprint.Name}";
        public IEnumerable<Tile> Tiles => building?.OccupiedTiles ?? placeholder!.OccupiedTiles;
        public Maybe<ISelectable> Selectable => placeholder != null
            ? Maybe.Just<ISelectable>(placeholder)
            : Maybe.Just<ISelectable>(building);
        public double PercentCompleted => building == null ? 0 : healthGiven / maxHealth;
        public bool CanAbort => placeholder != null;
        public bool Finished { get; private set; }

        public BuildingWorkerTask(
            Id<IWorkerTask> taskId,
            BuildingPlaceholder placeholder,
            FactionResources.IResourceReservation resourceReservation)
        {
            Id = taskId;
            this.placeholder = placeholder;
            blueprint = placeholder.Blueprint;
            resourceConsumer = new ResourceConsumer(placeholder.Game, resourceReservation, 0.ResourcesPerSecond());
        }

        public void SetBuilding(Building building)
        {
            DebugAssert.State.Satisfies(placeholder != null, "Placeholder needs to be set when building is set.");
            DebugAssert.State.Satisfies(this.building == null, "Can only set building once.");
            placeholder = null;
            this.building = building;
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
            if (placeholder != null && resourceConsumer.CanConsume)
            {
                onConstructionStart();
            }
            if (building?.Deleted ?? false)
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
            if (building != null)
            {
                updateBuildingToMatch();
            }
            if (resourceConsumer.IsDone)
            {
                building?.Sync(FinishBuildingConstruction.Command);
            }
        }

        public void OnAbort()
        {
            resourceConsumer.Abort();
            placeholder?.Delete();
            building?.Delete();
        }

        private void onConstructionStart()
        {
            placeholder!.Sync(StartBuildingConstruction.Command);
        }

        private void updateBuildingToMatch()
        {
            // Building was set to completed, probably because of command from server.
            // We want to keep consuming resources though to make sure the resources stay in sync.
            if (building!.IsBuildCompleted) return;

            var buildProgress = resourceConsumer.PercentageDone;
            DebugAssert.State.Satisfies(buildProgress <= 1);
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
