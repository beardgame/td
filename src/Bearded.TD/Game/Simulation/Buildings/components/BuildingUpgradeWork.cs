using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    // TODO: remove this component when it's done upgrading
    sealed class BuildingUpgradeWork<T> : Component<T>
        where T : IComponentOwner, IGameObject
    {
        private readonly IIncompleteUpgrade incompleteUpgrade;
        private FactionResources? resources;
        private ResourceConsumer? resourceConsumer;

        private bool started;
        private bool finished;

        public BuildingUpgradeWork(IIncompleteUpgrade incompleteUpgrade)
        {
            this.incompleteUpgrade = incompleteUpgrade;
        }

        protected override void Initialize()
        {
            ComponentDependencies.Depend<IOwnedByFaction>(Owner, Events, ownedByFaction =>
            {
                if (!ownedByFaction.Faction.TryGetBehaviorIncludingAncestors(out resources))
                {
                    throw new NotSupportedException("Cannot upgrade building without resources.");
                }
            });
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (resources == null || finished || (Owner is IBuilding building && !building.State.CanApplyUpgrades))
            {
                return;
            }

            if (incompleteUpgrade.IsCompleted)
            {
                resourceConsumer?.CompleteIfNeeded();
                finished = true;
                return;
            }

            if (incompleteUpgrade.IsCancelled)
            {
                resourceConsumer?.Abort();
                finished = true;
                return;
            }

            if (resourceConsumer == null)
            {
                var resourceReservation =
                    resources.ReserveResources(new FactionResources.ResourceRequest(incompleteUpgrade.Upgrade.Cost));
                resourceConsumer =
                    new ResourceConsumer(Owner.Game, resourceReservation, Constants.Game.Worker.UpgradeSpeed);
            }

            resourceConsumer.PrepareIfNeeded();
            if (!resourceConsumer.CanConsume)
            {
                return;
            }

            if (!started)
            {
                started = true;
                incompleteUpgrade.StartUpgrade();
            }

            resourceConsumer.Update();
            incompleteUpgrade.SetUpgradeProgress(resourceConsumer.PercentageDone);

            if (resourceConsumer.IsDone)
            {
                incompleteUpgrade.CompleteUpgrade();
            }
        }

        public override void Draw(CoreDrawers drawers) {}
    }
}
