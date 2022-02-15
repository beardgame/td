﻿using Bearded.TD.Content.Models;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingStateManager<T>
    : Component<T>,
        IBuildingStateProvider,
        IListener<ConstructionFinished>,
        IListener<ConstructionStarted>,
        IListener<EnactDeath>,
        IListener<ObjectRepaired>,
        IListener<ObjectRuined>,
        IListener<PreventPlayerHealthChanges>
    where T : IComponentOwner<T>, IDeletable, IGameObject
{
    private readonly BuildingState state = new();
    private IHealth? health;
    private ICost? cost;

    public IBuildingState State { get; }

    public BuildingStateManager()
    {
        State = state.CreateProxy();
    }

    protected override void OnAdded() {
        SelectionListener.Create(
                onFocus: () => state.SelectionState = SelectionState.Focused,
                onFocusReset: () => state.SelectionState = SelectionState.Default,
                onSelect: () => state.SelectionState = SelectionState.Selected,
                onSelectionReset: () => state.SelectionState = SelectionState.Default)
            .Subscribe(Events);
        Events.Subscribe<ConstructionFinished>(this);
        Events.Subscribe<ConstructionStarted>(this);
        Events.Subscribe<EnactDeath>(this);
        Events.Subscribe<ObjectRepaired>(this);
        Events.Subscribe<ObjectRuined>(this);
        Events.Subscribe<PreventPlayerHealthChanges>(this);

        ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);
        ComponentDependencies.Depend<ICost>(Owner, Events, c => cost = c);

        var ruinState = new FindObjectRuinState(false);
        Events.Preview(ref ruinState);
        state.IsRuined = ruinState.IsRuined;
    }

    public override void OnRemoved()
    {
        DebugAssert.State.IsInvalid("Building state should never be removed.");
    }

    public void HandleEvent(ConstructionFinished @event)
    {
        state.IsCompleted = true;
    }

    public void HandleEvent(ConstructionStarted @event)
    {
        materialize();
    }

    public void HandleEvent(EnactDeath @event)
    {
        state.IsDead = true;
    }

    public void HandleEvent(ObjectRepaired @event)
    {
        state.IsRuined = false;
    }

    public void HandleEvent(ObjectRuined @event)
    {
        state.IsRuined = true;
    }

    public void HandleEvent(PreventPlayerHealthChanges @event)
    {
        state.AcceptsPlayerHealthChanges = false;
    }

    private void materialize()
    {
        Owner.AddComponent(new Syncer<T>());
        state.IsMaterialized = true;
        Events.Send(new Materialized());
        ReportAggregator.Register(Events, new BuildingStateReport(Owner, this));
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (state.IsCompleted &&
            !state.IsRuined &&
            (health?.HealthPercentage ?? 1) < Constants.Game.Building.RuinedPercentage)
        {
            Owner.AddComponent(new Ruined<T>(new RuinedParametersTemplate(cost?.Resources / 2)));
        }

        if (state.IsDead)
        {
            // TODO(building): cast necessary right now
            (Owner as ComponentGameObject).Sync(DeleteGameObject.Command);
        }
    }

    private sealed class BuildingStateReport : IBuildingStateReport
    {
        private readonly T owner;
        private readonly IBuildingStateProvider buildingStateProvider;

        public ReportType Type => ReportType.EntityActions;

        public ComponentGameObject Building => (owner as ComponentGameObject)!;

        public bool CanBeDeleted => buildingStateProvider.State.IsMaterialized &&
            buildingStateProvider.State.AcceptsPlayerHealthChanges;

        public BuildingStateReport(T owner, IBuildingStateProvider buildingStateProvider)
        {
            this.owner = owner;
            this.buildingStateProvider = buildingStateProvider;
        }
    }
}

interface IBuildingStateReport : IReport
{
    // TODO(building): cast needed to get the ID
    ComponentGameObject Building { get; }
    bool CanBeDeleted { get; }
}
