using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [Component("worker")]
    // TODO: make generic
    sealed class WorkerComponent : Component<ComponentGameObject, IWorkerParameters>, ITileWalkerOwner, IWorkerComponent
    {
        private WorkerTaskManager? workerTaskManager;
        private WorkerState? currentState;
        private IEnumerable<Tile> taskTiles = Enumerable.Empty<Tile>();

        private TileWalker tileWalker = null!;

        public Tile CurrentTile => tileWalker.CurrentTile;
        public IFactioned HubOwner { get; private set; } = null!;
        public new IWorkerParameters Parameters => base.Parameters;

        public WorkerComponent(IWorkerParameters parameters) : base(parameters) { }

        protected override void Initialize()
        {
            Owner.FindInComponentOwnerTree<IFactioned>().Match(
                onValue: owner => HubOwner = owner,
                onNothing: () => throw new InvalidDataException());
            tileWalker = new TileWalker(this, Owner.Game.Level, Level.GetTile(Owner.Position));
            // Needs to be sent after tile walker is initialized to ensure CurrentTile is not null.
            Owner.Game.Meta.Events.Send(new WorkerAdded(this));
        }

        private void onDelete()
        {
            workerTaskManager?.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState?.Update(elapsedTime);
            tileWalker.Update(elapsedTime, Parameters.MovementSpeed);

            Owner.Position = tileWalker.Position.WithZ(0.1f);

            Owner.Deleting += onDelete;
        }

        public override void Draw(CoreDrawers drawers)
        {
            var sprites = Owner.Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")];
            var sprite =
                sprites.GetSprite("halo").MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, UVColorVertex.Create);

            sprite.Draw(Owner.Position.NumericValue, 0.5f, workerTaskManager?.WorkerColor ?? Color.White);
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
        IFactioned HubOwner { get; }
        IWorkerParameters Parameters { get; }
        void AssignToTaskManager(WorkerTaskManager faction);
        void AssignTask(IWorkerTask task);
        void SuspendCurrentTask();
    }
}
