using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingUpgradeWork : Component
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
            State.Satisfies(
                factionProvider == null && faction == null,
                "Should not initialize faction provider more than once.");

            factionProvider = provider;
            faction = provider.Faction;
        });
        Owner.Deleting += () => resourceConsumer?.Abort();
    }

    public override void Activate()
    {
        base.Activate();

        if (faction == null)
        {
            throw new InvalidOperationException("Faction must be resolved before activating this component.");
        }
        if (!faction.TryGetBehaviorIncludingAncestors(out resources))
        {
            throw new NotSupportedException("Cannot build building without resources.");
        }

        var resourceReservation =
            resources.ReserveResources(new FactionResources.ResourceRequest(incompleteUpgrade.Upgrade.Cost));
        resourceConsumer =
            new ResourceConsumer(Owner.Game, resourceReservation, Constants.Game.Worker.UpgradeSpeed);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (resources == null || resourceConsumer == null)
        {
            throw new InvalidOperationException("Component must be activated before being updated.");
        }

        if (finished || !(state?.CanApplyUpgrades ?? false))
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
        incompleteUpgrade.SetUpgradeProgress(resourceConsumer.PercentageDone, resourceConsumer.ResourcesClaimed);

        if (resourceConsumer.IsDone)
        {
            incompleteUpgrade.CompleteUpgrade();
        }
    }
}
