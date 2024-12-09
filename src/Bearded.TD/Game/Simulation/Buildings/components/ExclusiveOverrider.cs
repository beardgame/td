using System;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class ExclusiveOverrider<TOverride>
    : IListener<ManualOverrideStarted>, IListener<ManualOverrideEnded>
    where TOverride : ExclusiveOverrider<TOverride>.Override
{
    public abstract record Override(Action Cancel);
    public interface IOverrideImplementation
    {
        void OnOverrideStart(TOverride @override);
        void OnOverrideEnd(TOverride @override);
    }

    public static ExclusiveOverrider<TOverride> CreateSubscribed(
        GameObject owner, ComponentEvents events, IOverrideImplementation implementation)
    {
        var instance = new ExclusiveOverrider<TOverride>(events, implementation);

        ComponentDependencies.Depend<IFactionProvider>(owner, events, provider => instance.factionProvider = provider);
        ComponentDependencies.Depend<IBuildingStateProvider>(owner, events, provider => instance.buildingState = provider);
        events.Subscribe<ManualOverrideStarted>(instance);
        events.Subscribe<ManualOverrideEnded>(instance);

        return instance;
   }

    private readonly ComponentEvents events;
    private readonly IOverrideImplementation implementation;

    private bool anyOverrideActive;
    private TOverride? activeControl;

    private IFactionProvider? factionProvider;
    private IBuildingStateProvider? buildingState;

    private ExclusiveOverrider(ComponentEvents events, IOverrideImplementation implementation)
    {
        this.events = events;
        this.implementation = implementation;
    }

    public void Update()
    {
        if (activeControl == null)
            return;

        if (buildingState is { State.IsFunctional: false })
        {
            activeControl.Cancel();
        }
    }

    public bool CanBeOverriddenBy(Faction faction)
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

    public void StartOverride(TOverride control)
    {
        State.Satisfies(activeControl == null);
        activeControl = control;
        implementation.OnOverrideStart(control);
        events.Send(new ManualOverrideStarted());
    }

    public void EndOverride()
    {
        State.Satisfies(activeControl != null);
        implementation.OnOverrideEnd(activeControl!);
        events.Send(new ManualOverrideEnded());
        activeControl = null;
    }
}
