using System;
using System.Linq;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins;

[Component("ruined")]
sealed class Ruined
    : Component,
        IRuined,
        IListener<RepairCancelled>,
        IListener<RepairFinished>,
        IPreviewListener<FindObjectRuinState>
{
    private readonly Disposer disposer = new();

    private IBuildingStateProvider? buildingStateProvider;
    private IncompleteRepair? incompleteRepair;

    private bool deleteNextFrame;

    protected override void OnAdded()
    {
        disposer.AddDisposable(
            ComponentDependencies.Depend<IBuildingStateProvider>(
                Owner, Events, provider => buildingStateProvider = provider));
        Events.Send(new ObjectRuined());
        Events.Subscribe<RepairCancelled>(this);
        Events.Subscribe<RepairFinished>(this);
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        disposer.Dispose();
        Events.Send(new ObjectRepaired());
        Events.Unsubscribe<RepairCancelled>(this);
        Events.Unsubscribe<RepairFinished>(this);
        Events.Unsubscribe(this);
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
            Owner.GetTilePresence().OccupiedTiles.Any(t => Owner.Game.VisibilityLayer[t].IsRevealed());
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
        @event = new FindObjectRuinState(IsRuined: true);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (deleteNextFrame)
        {
            Owner.RemoveComponent(this);
        }
    }
}

interface IRuined
{
    bool CanBeRepairedBy(Faction faction);
    IIncompleteRepair StartRepair(Faction repairingFaction);
}
