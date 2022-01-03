using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Workers;

[Component("workerAntenna")]
sealed class WorkerAntenna<T>
    : Component<T, IWorkerAntennaParameters>, IListener<ObjectDeleting>, IWorkerAntenna, IDrawableComponent
    where T : IComponentOwner, IGameObject, IPositionable
{
    private IBuildingState? state;
    private IFactionProvider? factionProvider;
    private Faction? initializedFaction;
    private WorkerNetwork? workerNetwork;
    private bool isInitialized;
    private TileRangeDrawer? fullNetworkDrawer;
    private TileRangeDrawer? localNetworkDrawer;

    public Position2 Position => Owner.Position.XY();

    public Unit WorkerRange { get; private set; }

    public WorkerAntenna(IWorkerAntennaParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider, IBuildingStateProvider>(Owner, Events, (provider, p) =>
        {
            factionProvider = provider;
            state = p.State;

            fullNetworkDrawer = new TileRangeDrawer(
                Owner.Game,
                () => state.RangeDrawing,
                () => TileAreaBorder.From(Owner.Game.Level, t => workerNetwork?.IsInRange(t) ?? false),
                Color.DodgerBlue);

            localNetworkDrawer = new TileRangeDrawer(
                Owner.Game,
                () => state.RangeDrawing,
                () => TileAreaBorder.From(((IWorkerAntenna)this).Coverage),
                Color.Orange);
        });
        initializeIfOwnerIsCompletedBuilding();
    }

    private void initializeIfOwnerIsCompletedBuilding()
    {
        if (state?.IsFunctional ?? false)
            initializeInternal();
    }

    private void initializeInternal()
    {
        State.Satisfies(factionProvider != null);
        initializedFaction = factionProvider!.Faction;
        initializedFaction.TryGetBehaviorIncludingAncestors(out workerNetwork);
        if (workerNetwork == null)
        {
            Owner.Game.Meta.Logger.Debug?.Log(
                "Initializing worker antenna for building without faction is a no-op.");
        }

        WorkerRange = Parameters.WorkerRange;
        workerNetwork?.RegisterAntenna(this);
        isInitialized = true;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (isInitialized && factionProvider?.Faction != initializedFaction)
        {
            workerNetwork?.UnregisterAntenna(this);
            initializedFaction = null;
            workerNetwork = null;
            isInitialized = false;
        }

        if (!isInitialized)
        {
            initializeIfOwnerIsCompletedBuilding();
        }

        if (Parameters.WorkerRange != WorkerRange)
        {
            WorkerRange = Parameters.WorkerRange;
            workerNetwork?.OnAntennaRangeUpdated();
        }
    }

    public void HandleEvent(ObjectDeleting @event)
    {
        workerNetwork?.UnregisterAntenna(this);
    }

    public void Draw(IComponentDrawer drawer)
    {
        fullNetworkDrawer?.Draw();
        localNetworkDrawer?.Draw();
    }
}
