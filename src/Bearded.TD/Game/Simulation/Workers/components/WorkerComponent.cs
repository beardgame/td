using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers;

[Component("worker")]
sealed class WorkerComponent : Component<WorkerComponent.IParameters>, ITileWalkerOwner, IWorkerComponent,
    IListener<DrawComponents>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(10, Type = AttributeType.MovementSpeed)]
        Speed MovementSpeed { get; }

        ResourceRate BuildingSpeed { get; }
    }

    private IFactionProvider? factionProvider;
    private WorkerTaskManager? workerTaskManager;
    private WorkerState? currentState;
    private IEnumerable<Tile> taskTiles = Enumerable.Empty<Tile>();

    private TileWalker tileWalker = null!;
    private SpriteDrawInfo<UVColorVertex, Color> sprite;

    public Faction? Faction { get; private set; }
    public Tile CurrentTile => tileWalker.CurrentTile;
    public new IParameters Parameters => base.Parameters;

    public WorkerComponent(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IFactionProvider>(Owner, Events, provider =>
        {
            factionProvider = provider;
            Faction = provider.Faction;
        });
    }

    public override void Activate()
    {
        base.Activate();

        tileWalker = new TileWalker(this, Owner.Game.Level, Level.GetTile(Owner.Position));

        sprite = SpriteDrawInfo.ForUVColor(Owner.Game,
            Owner.Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")].GetSprite("halo"));
        Events.Subscribe(this);

        Owner.Game.Meta.Events.Send(new WorkerAdded(this));
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    private void onDelete()
    {
        workerTaskManager?.UnregisterWorker(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (factionProvider?.Faction != Faction)
        {
            State.IsInvalid("Workers cannot change their faction.");
        }

        currentState?.Update(elapsedTime);
        tileWalker.Update(elapsedTime, Parameters.MovementSpeed);

        Owner.Position = tileWalker.Position.WithZ(0.1f);

        Owner.Deleting += onDelete;
    }

    public void HandleEvent(DrawComponents e)
    {
        e.Drawer.DrawSprite(sprite, Owner.Position.NumericValue, 0.5f, 0,
            workerTaskManager?.WorkerColor ?? Color.White);
    }

    public void AssignToTaskManager(WorkerTaskManager workerTaskManager)
    {
        if (this.workerTaskManager != null)
        {
            this.workerTaskManager.UnregisterWorker(this);
            this.workerTaskManager = null;
        }

        this.workerTaskManager = workerTaskManager;
        workerTaskManager.RegisterWorker(this);

        setState(WorkerState.Idle(workerTaskManager, this));
    }

    public void AssignTask(IWorkerTask task)
    {
        if (workerTaskManager == null)
        {
            throw new InvalidOperationException("Cannot assign tasks to a worker not assigned to a task manager");
        }
        setState(WorkerState.ExecuteTask(workerTaskManager, this, task));
    }

    public void SuspendCurrentTask()
    {
        if (workerTaskManager == null)
        {
            throw new InvalidOperationException("Cannot suspend tasks for a worker not assigned to a task manager");
        }
        setState(WorkerState.Idle(workerTaskManager, this));
    }

    private void setState(WorkerState newState)
    {
        if (currentState != null)
        {
            currentState.Stop();
            currentState.StateChanged -= setState;
            currentState.TaskTilesChanged -= setTaskTiles;
        }
        currentState = newState;
        currentState.StateChanged += setState;
        currentState.TaskTilesChanged += setTaskTiles;
        currentState.Start();
    }

    private void setTaskTiles(IEnumerable<Tile> newTaskTiles)
    {
        taskTiles = newTaskTiles;
    }

    public void OnTileChanged(Tile oldTile, Tile newTile) { }

    public Direction GetNextDirection()
    {
        if (currentState == null || taskTiles.IsNullOrEmpty() || CurrentTile.NeighboursToTiles(taskTiles))
        {
            return Direction.Unknown;
        }

        var goalTile = taskTiles.MinBy(tile => tile.DistanceTo(CurrentTile));
        var diff = Level.GetPosition(goalTile) - tileWalker.Position;
        return diff.Direction.Hexagonal();
    }
}

interface IWorkerComponent
{
    Tile CurrentTile { get; }
    Faction Faction { get; }
    WorkerComponent.IParameters Parameters { get; }
    void AssignToTaskManager(WorkerTaskManager faction);
    void AssignTask(IWorkerTask task);
    void SuspendCurrentTask();
}
