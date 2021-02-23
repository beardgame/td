using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [Component("worker")]
    // TODO: make generic
    sealed class WorkerComponent : Component<Worker>, ITileWalkerOwner, IWorkerComponent
    {
        private Faction? faction;
        private WorkerState? currentState;
        private IEnumerable<Tile> taskTiles = Enumerable.Empty<Tile>();

        private TileWalker tileWalker = null!;

        public Tile CurrentTile => tileWalker.CurrentTile;
        public IFactioned HubOwner => Owner.HubOwner;

        protected override void Initialize()
        {
            tileWalker = new TileWalker(this, Owner.Game.Level, Tile.Origin);
            // Needs to be sent after tile walker is initialized to ensure CurrentTile is not null.
            Owner.Game.Meta.Events.Send(new WorkerAdded(this));
        }

        private void onDelete()
        {
            faction?.Workers?.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState?.Update(elapsedTime);
            tileWalker.Update(elapsedTime, Constants.Game.Worker.MovementSpeed);

            Owner.Position = tileWalker.Position;
            Owner.CurrentTile = tileWalker.CurrentTile;

            Owner.Deleting += onDelete;
        }

        public override void Draw(CoreDrawers drawers)
        {
            var sprites = Owner.Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")];
            var sprite =
                sprites.GetSprite("halo").MakeConcreteWith(Owner.Game.Meta.SpriteRenderers, UVColorVertex.Create);

            sprite.Draw(Owner.Position.NumericValue.WithZ(0.1f), 0.5f, faction?.Color ?? Color.White);
        }

        public void AssignToFaction(Faction faction)
        {
            if (faction.Workers == null)
            {
                throw new InvalidOperationException("Cannot assign worker to a faction without worker manager");
            }

            if (this.faction != null)
            {
                this.faction.Workers!.UnregisterWorker(this);
                this.faction = null;
            }

            this.faction = faction;
            this.faction.Workers!.RegisterWorker(this);

            setState(WorkerState.Idle(this.faction.Workers, this));
        }

        public void AssignTask(IWorkerTask task)
        {
            if (faction == null)
            {
                throw new InvalidOperationException("Cannot assign tasks to a worker not assigned to a faction");
            }
            setState(WorkerState.ExecuteTask(faction.Workers!, this, task));
        }

        public void SuspendCurrentTask()
        {
            if (faction == null)
            {
                throw new InvalidOperationException("Cannot suspend tasks for a worker not assigned to a faction");
            }
            setState(WorkerState.Idle(faction.Workers!, this));
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
        void AssignToFaction(Faction faction);
        void AssignTask(IWorkerTask task);
        void SuspendCurrentTask();
    }
}
