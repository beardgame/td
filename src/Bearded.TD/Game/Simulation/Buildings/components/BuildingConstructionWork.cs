using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingConstructionWork : Component
{
    private readonly IIncompleteBuilding incompleteBuilding;

    private IFactionProvider? factionProvider;
    private Faction? faction;
    private FactionResources? resources;
    private ResourceConsumer? resourceConsumer;

    private bool started;
    private bool finished;

    public ResourceAmount? ResourcesInvestedSoFar => resourceConsumer?.ResourcesClaimed;

    public BuildingConstructionWork(IIncompleteBuilding incompleteBuilding)
    {
        this.incompleteBuilding = incompleteBuilding;
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider =>
        {
            State.Satisfies(
                factionProvider == null && faction == null,
                "Should not initialize faction provider more than once.");

            factionProvider = provider;
            faction = provider.Faction;
        });
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

        var cost = Owner.GetComponents<ICost>().SingleOrDefault()?.Resources ?? ResourceAmount.Zero;
        var resourceReservation =
            resources.ReserveResources(new FactionResources.ResourceRequest(cost));
        resourceConsumer =
            new ResourceConsumer(Owner.Game, resourceReservation, Constants.Game.Worker.UpgradeSpeed);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (resources == null || resourceConsumer == null)
        {
            throw new InvalidOperationException("Component must be activated before being updated.");
        }

        if (finished)
        {
            return;
        }

        if (incompleteBuilding.IsCompleted)
        {
            resourceConsumer?.CompleteIfNeeded();
            finished = true;
            Owner.RemoveComponent(this);
            return;
        }

        if (incompleteBuilding.IsCancelled)
        {
            resourceConsumer?.Abort();
            finished = true;
            Owner.RemoveComponent(this);
            return;
        }

        if (factionProvider?.Faction != faction)
        {
            State.IsInvalid(
                "Did not expect the faction provider to ever provide a different faction during the construction " +
                "work.");
        }

        resourceConsumer.PrepareIfNeeded();
        if (!resourceConsumer.CanConsume)
        {
            return;
        }

        if (!started)
        {
            started = true;
            incompleteBuilding.StartBuild();
        }

        resourceConsumer.Update();
        incompleteBuilding.SetBuildProgress(resourceConsumer.PercentageDone);

        if (resourceConsumer.IsDone)
        {
            incompleteBuilding.CompleteBuild();
        }
    }
}
