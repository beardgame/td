using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Selection;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingStateManager : Component,
        IBuildingStateProvider,
        IBreakageHandler,
        IListener<ConstructionFinished>,
        IListener<ConstructionStarted>,
        IListener<ObjectKilled>,
        IListener<ObjectRepaired>,
        IListener<ObjectRuined>,
        IListener<PreventPlayerHealthChanges>,
        IListener<PreventRuin>
{
    private readonly BuildingState state = new();
    private IHealth? health;
    private IBreakageReceipt? ruinedBreakage;
    private bool ruinPrevented;

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
        Events.Subscribe<ObjectKilled>(this);
        Events.Subscribe<ObjectRepaired>(this);
        Events.Subscribe<ObjectRuined>(this);
        Events.Subscribe<PreventPlayerHealthChanges>(this);
        Events.Subscribe<PreventRuin>(this);

        ReportAggregator.Register(Events, new BuildingStateReport(Owner, this));

        ComponentDependencies.Depend<IHealth>(Owner, Events, h => health = h);

        var ruinState = new FindObjectRuinState(false);
        Events.Preview(ref ruinState);
        if (ruinState.IsRuined && !ruinPrevented)
        {
            ruinedBreakage = BreakObject();
        }
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

    public void HandleEvent(ObjectKilled @event)
    {
        state.IsDead = true;
    }

    public void HandleEvent(ObjectRepaired @event)
    {
        ruinedBreakage?.Repair();
        ruinedBreakage = null;
    }

    public void HandleEvent(ObjectRuined @event)
    {
        if (ruinPrevented) return;
        ruinedBreakage ??= BreakObject();
    }

    public void HandleEvent(PreventPlayerHealthChanges @event)
    {
        state.AcceptsPlayerHealthChanges = false;
    }

    public void HandleEvent(PreventRuin @event)
    {
        ruinedBreakage?.Repair();
        ruinedBreakage = null;
        ruinPrevented = true;
    }

    public IBreakageReceipt BreakObject()
    {
        var breakage = new ActiveBreakage(state);
        state.ActiveBreakages.Add(breakage);
        return breakage;
    }

    private void materialize()
    {
        Owner.AddComponent(new Syncer());
        state.IsMaterialized = true;
        Events.Send(new Materialized());
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (state.IsCompleted &&
            ruinedBreakage == null &&
            !ruinPrevented &&
            (health?.HealthPercentage ?? 1) < Constants.Game.Building.RuinedPercentage)
        {
            Owner.Sync(RuinBuilding.Command);
        }
    }

    private sealed class BuildingStateReport : IBuildingStateReport
    {
        private readonly IBuildingStateProvider buildingStateProvider;

        public ReportType Type => ReportType.EntityActions;

        public GameObject Building { get; }

        public bool IsMaterialized => buildingStateProvider.State.IsMaterialized;

        public bool CanBeDeleted => buildingStateProvider.State.AcceptsPlayerHealthChanges;
        public ResourceAmount RefundValue => Building.TotalResourcesInvested() ?? ResourceAmount.Zero;

        public BuildingStateReport(GameObject owner, IBuildingStateProvider buildingStateProvider)
        {
            Building = owner;
            this.buildingStateProvider = buildingStateProvider;
        }
    }

    private sealed class ActiveBreakage : IBreakageReceipt
    {
        private readonly BuildingState mutableState;

        public ActiveBreakage(BuildingState mutableState)
        {
            this.mutableState = mutableState;
        }

        public void Repair()
        {
            mutableState.ActiveBreakages.Remove(this);
        }
    }
}

interface IBuildingStateReport : IReport
{
    GameObject Building { get; }
    bool IsMaterialized { get; }
    bool CanBeDeleted { get; }
    ResourceAmount RefundValue { get; }
}
