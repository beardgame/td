using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class BuildingWork : Component, IListener<ObjectDeleting>
{
    private readonly IIncompleteWork work;

    private IFactionProvider? factionProvider;
    private Faction? faction;
    private FactionResources? resources;
    private ResourceConsumer? resourceConsumer;

    private bool started;
    private bool finished;

    protected abstract ResourceAmount Cost { get; }
    protected abstract ResourceRate ConsumptionRate { get; }

    protected ResourceAmount? ResourcesInvestedSoFar => resourceConsumer?.ResourcesClaimed;

    protected BuildingWork(IIncompleteWork work)
    {
        this.work = work;
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
        Events.Subscribe(this);
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
            resources.ReserveResources(new FactionResources.ResourceRequest(Cost));
        resourceConsumer =
            new ResourceConsumer(Owner.Game, resourceReservation, ConsumptionRate);
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

        if (work.IsCompleted)
        {
            resourceConsumer?.CompleteIfNeeded();
            finished = true;
            Owner.RemoveComponent(this);
            return;
        }

        if (work.IsCancelled)
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
            work.StartWork();
        }

        resourceConsumer.Update();
        work.SetWorkProgress(resourceConsumer.PercentageDone, resourceConsumer.ResourcesClaimed);

        if (resourceConsumer.IsDone)
        {
            work.CompleteWork();
        }
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        base.OnRemoved();
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        resourceConsumer?.Abort();
    }
}
