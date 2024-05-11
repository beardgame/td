using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

abstract class AllowManualOverride<T>
    : Component, IListener<ManualOverrideStarted>, IListener<ManualOverrideEnded> where T : AllowManualOverride<T>.Override
{
    public abstract record Override(Action Cancel);

    private bool anyOverrideActive;
    private T? activeControl;

    private IFactionProvider? factionProvider;
    private IBuildingStateProvider? buildingState;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider => factionProvider = provider);
        ComponentDependencies.Depend<IBuildingStateProvider>(Owner, Events, provider => buildingState = provider);
        Events.Subscribe<ManualOverrideStarted>(this);
        Events.Subscribe<ManualOverrideEnded>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (activeControl == null)
            return;

        if (buildingState is { State.IsFunctional: false })
        {
            activeControl.Cancel();
        }
    }

    protected bool CanBeOverriddenBy(Faction faction)
    {
        if (anyOverrideActive || buildingState is { State.IsFunctional: false })
        {
            return false;
        }

        return factionProvider != null && factionProvider.Faction.OwnedBuildingsCanBeManuallyControlledBy(faction);
    }

    public void HandleEvent(ManualOverrideStarted @event)
    {
        State.Satisfies(!anyOverrideActive);
        anyOverrideActive = true;
    }

    public void HandleEvent(ManualOverrideEnded @event)
    {
        State.Satisfies(anyOverrideActive);
        anyOverrideActive = false;
    }

    protected void StartOverride(T control)
    {
        State.Satisfies(activeControl == null);
        activeControl = control;
        OnOverrideStart(control);
        Events.Send(new ManualOverrideStarted());
    }

    protected abstract void OnOverrideStart(T control);

    protected void EndOverride()
    {
        State.Satisfies(activeControl != null);
        OnOverrideEnd(activeControl!);
        Events.Send(new ManualOverrideEnded());
        activeControl = null;
    }

    protected abstract void OnOverrideEnd(T control);
}
