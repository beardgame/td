using amulware.Graphics;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Resources
{
    class Worker : GameObject, ITileWalkerOwner
    {
        private readonly WorkerManager manager;
        private TileWalker tileWalker;

        public Faction Faction { get; }
        public double WorkerSpeed => Constants.Game.Worker.WorkerSpeed;
        public Squared<Unit> WorkRadiusSquared = Constants.Game.Worker.WorkRadiusSquared;

        public Position2 Position => tileWalker?.Position ?? Position2.Zero;

        private WorkerState currentState;
        private Position2 goalPosition;

        public Worker(WorkerManager manager, Faction faction)
        {
            this.manager = manager;
            Faction = faction;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            tileWalker = new TileWalker(this, Game.Level, 3.UnitsPerSecond());
            tileWalker.Teleport(Position2.Zero, Game.Level.GetTile(Position2.Zero));

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
                currentState.GoalPositionChanged -= setGoalPosition;
            }
            currentState = newState;
            currentState.StateChanged += setState;
            currentState.GoalPositionChanged += setGoalPosition;
            currentState.Start();
        }

        private void setGoalPosition(Position2 goalPos)
        {
            goalPosition = goalPos;
        }

        protected override void OnDelete()
        {
            base.OnDelete();

            manager.UnregisterWorker(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            currentState.Update(elapsedTime);
            tileWalker.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.DeepPink;
            geo.DrawCircle(Position.NumericValue, .3f * Constants.Game.World.HexagonDiameter);
        }

        public void OnTileChanged(Tile<TileInfo> oldTile, Tile<TileInfo> newTile) { }

        public Direction GetNextDirection()
        {
            if (currentState == null) return Direction.Unknown;

            var goalTile = Game.Level.GetTile(goalPosition);
            if (tileWalker.CurrentTile == goalTile) return Direction.Unknown;

            var diff = goalPosition - Position;
            return diff.Direction.Hexagonal();
        }
    }
}
