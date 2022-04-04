using System;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Workers;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

[Component("ruined")]
sealed class Ruined
    : Component<IRuinedParameters>,
        IRuined,
        IListener<RepairCancelled>,
        IListener<RepairFinished>,
        IPreviewListener<FindObjectRuinState>
{
    private readonly Disposer disposer = new();

    private IBuildingStateProvider? buildingStateProvider;
    private readonly OccupiedTilesTracker occupiedTilesTracker = new();
    private ReportAggregator.IReportHandle? reportHandle;
    private IncompleteRepair? incompleteRepair;

    private bool hasRepairCost => Parameters.RepairCost.HasValue;
    private bool deleteNextFrame;

    public ResourceAmount? RepairCost => Parameters.RepairCost;

    public Ruined(IRuinedParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        disposer.AddDisposable(
            ComponentDependencies.Depend<IBuildingStateProvider>(
                Owner, Events, provider => buildingStateProvider = provider));
        occupiedTilesTracker.Initialize(Owner, Events);
        Events.Send(new ObjectRuined());
        Events.Subscribe<RepairCancelled>(this);
        Events.Subscribe<RepairFinished>(this);
        Events.Subscribe(this);
        if (hasRepairCost)
        {
            reportHandle = ReportAggregator.Register(Events, new RuinedReport(this));
        }
    }

    public override void OnRemoved()
    {
        disposer.Dispose();
        occupiedTilesTracker.Dispose(Events);
        Events.Send(new ObjectRepaired());
        Events.Unsubscribe<RepairCancelled>(this);
        Events.Unsubscribe<RepairFinished>(this);
        Events.Unsubscribe(this);
        reportHandle?.Unregister();
        reportHandle = null;
        if (incompleteRepair != null)
        {
            Owner.RemoveComponent(incompleteRepair);
        }
        base.OnRemoved();
    }

    public IIncompleteRepair StartRepair(Faction repairingFaction)
    {
        if (incompleteRepair != null)
        {
            throw new InvalidOperationException("Only one repair attempt can be in progress at a time.");
        }

        incompleteRepair = new IncompleteRepair(repairingFaction);
        Owner.AddComponent(incompleteRepair);
        return incompleteRepair;
    }

    public bool CanBeRepairedBy(Faction faction)
    {
        // For the time being, we only allow one faction to repair the building at a time, so if an incomplete
        // repair already exists, we disable any new repair attempts. Cancelling a repair will remove the incomplete
        // repair component, resetting this state.
        return incompleteRepair == null &&
            (buildingStateProvider?.State.AcceptsPlayerHealthChanges ?? false) &&
            faction.TryGetBehaviorIncludingAncestors<FactionResources>(out _) &&
            faction.TryGetBehaviorIncludingAncestors<WorkerTaskManager>(out _) &&
            occupiedTilesTracker.OccupiedTiles.Any(t => Owner.Game.VisibilityLayer[t].IsRevealed());
    }

    public void HandleEvent(RepairCancelled @event)
    {
        if (incompleteRepair != null)
        {
            Owner.RemoveComponent(incompleteRepair);
            incompleteRepair = null;
        }
    }

    public void HandleEvent(RepairFinished @event)
    {
        Events.Send(new ConvertToFaction(@event.RepairingFaction));
        // We need to defer deletion because we want to unsubscribe from this event.
        deleteNextFrame = true;
    }

    public void PreviewEvent(ref FindObjectRuinState @event)
    {
        @event = @event with { IsRuined = true };
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (deleteNextFrame)
        {
            Owner.RemoveComponent(this);
        }
    }

    private sealed class RuinedReport : IRuinedReport
    {
        private readonly Ruined instance;

        public ReportType Type => ReportType.EntityActions;

        public RuinedReport(Ruined instance)
        {
            this.instance = instance;
        }

        public ResourceAmount RepairCost => instance.Parameters.RepairCost ??
            throw new InvalidDataException(
                "Ruined reports should not exist for ruined components without repair cost.");

        public GameObject Building => instance.Owner;
        public bool RepairInProgress => instance.incompleteRepair != null;
        public double PercentageComplete => instance.incompleteRepair?.PercentageComplete ?? 0;
        public bool CanBeRepairedBy(Faction faction) => instance.CanBeRepairedBy(faction);
    }
}

interface IRuined
{
    ResourceAmount? RepairCost { get; }
    bool CanBeRepairedBy(Faction faction);
    IIncompleteRepair StartRepair(Faction repairingFaction);
}

interface IRuinedReport : IReport
{
    GameObject Building { get; }
    ResourceAmount RepairCost { get; }
    bool RepairInProgress { get; }
    double PercentageComplete { get; }

    bool CanBeRepairedBy(Faction faction);
}
