using System.Collections.Generic;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.GameState.Components.Damage;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Game.GameState.Workers;
using Bearded.TD.Game.Meta;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Buildings
{
    sealed class BuildingWorkerTask : IWorkerTask, IResourceConsumer
    {
        private readonly IBuildingBlueprint blueprint;

        private BuildingPlaceholder placeholder;
        private Building building;

        private ResourceAmount resourcesConsumed;
        private int healthGiven = 1;
        private int maxHealth = 1;

        public Id<IWorkerTask> Id { get; }
        public string Name => $"Build {blueprint.Name}";
        public IEnumerable<Tile> Tiles => building?.OccupiedTiles ?? placeholder?.OccupiedTiles;
        public Maybe<ISelectable> Selectable => placeholder != null
            ? Maybe.Just<ISelectable>(placeholder)
            : Maybe.Just<ISelectable>(building);
        public double PercentCompleted => building == null ? 0 : (double) healthGiven / maxHealth;
        public bool CanAbort => placeholder != null;
        public bool Finished { get; private set; }

        public BuildingWorkerTask(Id<IWorkerTask> taskId, BuildingPlaceholder placeholder)
        {
            Id = taskId;
            this.placeholder = placeholder;
            blueprint = placeholder.Blueprint;
        }

        public void SetBuilding(Building building)
        {
            DebugAssert.State.Satisfies(placeholder != null, "Placeholder needs to be set when building is set.");
            DebugAssert.State.Satisfies(this.building == null, "Can only set building once.");
            // ReSharper disable once PossibleNullReferenceException
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

            var healthRemaining = maxHealth - healthGiven;
            building.SetBuildProgress(1, healthRemaining);
            building.Completing -= onBuildingCompleting;
            Finished = true;
        }

        public void Progress(TimeSpan elapsedTime, ResourceManager resourceManager, ResourceRate ratePerS)
        {
            if (placeholder != null)
            {
                onConstructionStart();
            }
            if (building?.Deleted ?? false)
            {
                building.Completing -= onBuildingCompleting;
                Finished = true;
                return;
            }

            var remaining = blueprint.ResourceCost - resourcesConsumed;
            resourceManager.RegisterConsumer(this, ratePerS, remaining);
        }

        public void OnAbort()
        {
            placeholder?.Delete();
            building?.Delete();
        }

        private void onConstructionStart()
        {
            placeholder.Sync(StartBuildingConstruction.Command);
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
                Finished = true;
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
            var expectedHealthGiven = Mathf.CeilToInt(buildProgress * maxHealth);
            var newHealthGiven = expectedHealthGiven - healthGiven;
            building.SetBuildProgress(buildProgress, newHealthGiven);
            healthGiven = expectedHealthGiven;
        }
    }
}