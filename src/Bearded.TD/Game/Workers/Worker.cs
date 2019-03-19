using System.Collections.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Workers
{
    class Worker : GameObject, ITileWalkerOwner
    {
        private readonly WorkerManager manager;
        private TileWalker tileWalker;

        public Faction Faction { get; }
        public double WorkerSpeed => Constants.Game.Worker.WorkerSpeed;
        public Squared<Unit> WorkRadiusSquared = Constants.Game.Worker.WorkRadiusSquared;

        public Position2 Position => tileWalker?.Position ?? Position2.Zero;
        public Tile CurrentTile => tileWalker?.CurrentTile ?? Level.GetTile(Position2.Zero);

        private WorkerState currentState;
        private IEnumerable<Tile> taskTiles;

        public Worker(WorkerManager manager, Faction faction)
        {
            this.manager = manager;
            Faction = faction;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            tileWalker = new TileWalker(this, Game.Level, Tile.Origin);

            manager.RegisterWorker(this);
            setState(WorkerState.Idle(manager, this));
        }

        public void AssignTask(WorkerTask task)
        {
            setState(WorkerState.ExecuteTask(manager, this, task));
        }

        private void setState(WorkerState newState)
        {
            if (currentState != null)
            {
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

            manager.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState.Update(elapsedTime);
            tileWalker.Update(elapsedTime, 3.UnitsPerSecond());
        }

        public override void Draw(GeometryManager geometries)
        {
            var sprites = Game.Meta.Blueprints.Sprites["particle"];
            var sprite = sprites.Sprites.GetSprite("halo");

            sprite.Draw(Position.NumericValue.WithZ(0), Faction.Color, 0.5f);
        }

        public void OnTileChanged(Tile oldTile, Tile newTile) { }

        public Direction GetNextDirection()
        {
            if (currentState == null || taskTiles.IsNullOrEmpty() || CurrentTile.NeighboursToTiles(taskTiles))
            {
                return Direction.Unknown;
            }

            var goalTile = taskTiles.MinBy(tile => tile.DistanceTo(CurrentTile));
            var diff = Level.GetPosition(goalTile) - Position;
            return diff.Direction.Hexagonal();
        }
    }
}
