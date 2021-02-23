using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Workers
{
    [ComponentOwner]
    sealed class Worker : GameObject, IComponentOwner<Worker>, ITileWalkerOwner, ISelectable
    {
        private readonly ComponentEvents events = new();
        private readonly ComponentCollection<Worker> components;

        private TileWalker? tileWalker;

        // TODO: this should be the worker hub
        public Maybe<IComponentOwner> Parent { get; } = Maybe.Nothing;

        public IFactioned HubOwner { get; }
        private Faction? faction;

        private Position2 position => tileWalker?.Position ?? Position2.Zero;
        public Tile CurrentTile => tileWalker?.CurrentTile ?? Level.GetTile(Position2.Zero);

        public SelectionState SelectionState { get; private set; }

        private WorkerState? currentState;
        private IEnumerable<Tile> taskTiles = Enumerable.Empty<Tile>();

        public Worker(IFactioned hubOwner)
        {
            components = new ComponentCollection<Worker>(this, events);

            HubOwner = hubOwner;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            tileWalker = new TileWalker(this, Game.Level, Tile.Origin);

            Game.ListAs(this);

            Game.Meta.Events.Send(new WorkerAdded(this));
        }

        public void AssignToFaction(Faction faction)
        {
            if (faction.Workers == null)
            {
                throw new InvalidOperationException("Cannot assign worker to a faction without worker manager");
            }
            if (faction.Resources == null)
            {
                throw new InvalidOperationException("Cannot assign worker to a faction without resources");
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
            setState(WorkerState.ExecuteTask(faction.Workers!, this, faction.Resources!, task));
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

        protected override void OnDelete()
        {
            base.OnDelete();

            faction?.Workers?.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState?.Update(elapsedTime);
            tileWalker?.Update(elapsedTime, 3.UnitsPerSecond());
        }

        public override void Draw(CoreDrawers drawers)
        {
            var sprites = Game.Meta.Blueprints.Sprites[ModAwareId.ForDefaultMod("particle")];
            var sprite = sprites.GetSprite("halo").MakeConcreteWith(Game.Meta.SpriteRenderers, UVColorVertex.Create);

            sprite.Draw(position.NumericValue.WithZ(0.1f), 0.5f, faction?.Color ?? Color.White);
        }

        public void OnTileChanged(Tile oldTile, Tile newTile) { }

        public Direction GetNextDirection()
        {
            if (currentState == null || taskTiles.IsNullOrEmpty() || CurrentTile.NeighboursToTiles(taskTiles))
            {
                return Direction.Unknown;
            }

            var goalTile = taskTiles.MinBy(tile => tile.DistanceTo(CurrentTile));
            var diff = Level.GetPosition(goalTile) - position;
            return diff.Direction.Hexagonal();
        }

        public void ResetSelection()
        {
            SelectionState = SelectionState.Default;
        }

        public void Focus(SelectionManager selectionManager) {}

        public void Select(SelectionManager selectionManager)
        {
            selectionManager.CheckCurrentlySelected(this);
            SelectionState = SelectionState.Selected;
        }

        IEnumerable<TComponent> IComponentOwner<Worker>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}
