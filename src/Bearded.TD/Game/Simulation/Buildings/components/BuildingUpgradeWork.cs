using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings
{
    sealed class BuildingUpgradeWork<T> : Component<T>
        where T : IComponentOwner<T>, IGameObject
    {
        private readonly IIncompleteUpgrade incompleteUpgrade;
        private IBuildingState? state;
        private IFactionProvider? factionProvider;
        private Faction? faction;
        private FactionResources? resources;
        private ResourceConsumer? resourceConsumer;

        private bool started;
        private bool finished;

        public BuildingUpgradeWork(IIncompleteUpgrade incompleteUpgrade)
        {
            this.incompleteUpgrade = incompleteUpgrade;
        }

        protected override void OnAdded()
        {
            ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, p => state = p.State);
            ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider =>
            {
                factionProvider = provider;
                faction = provider.Faction;
                if (!faction.TryGetBehaviorIncludingAncestors(out resources))
                {
                    throw new NotSupportedException("Cannot upgrade building without resources.");
                }
            });
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (resources == null || finished || !(state?.CanApplyUpgrades ?? false))
            {
                return;
            }

            if (incompleteUpgrade.IsCompleted)
            {
                resourceConsumer?.CompleteIfNeeded();
                finished = true;
                Owner.RemoveComponent(this);
                return;
            }

            if (incompleteUpgrade.IsCancelled)
            {
                resourceConsumer?.Abort();
                finished = true;
                Owner.RemoveComponent(this);
                return;
            }

            if (factionProvider?.Faction != faction)
            {
                State.IsInvalid(
                    "Did not expect the faction provider to ever provide a different faction during the upgrade work.");
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
