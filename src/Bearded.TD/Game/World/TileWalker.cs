using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.World
{
    sealed class TileWalker
    {
        private readonly ITileWalkerOwner owner;
        private readonly Level level;

        public Position2 Position { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }
        private Position2 currentTilePosition => level.GetPosition(CurrentTile);
        private Tile<TileInfo> goalTile;
        private Position2 goalPosition => level.GetPosition(goalTile);

        public Tile<TileInfo> GoalTile => goalTile;
        public bool IsMoving { get; private set; }

        public TileWalker(ITileWalkerOwner owner, Level level)
        {
            this.owner = owner;
            this.level = level;
        }

        public void Update(TimeSpan elapsedTime, Speed currentSpeed)
        {
            updateMovement(elapsedTime, currentSpeed);
            updateCurrentTileIfNeeded();
        }

        public void Teleport(Position2 newPos, Tile<TileInfo> goalTile)
        {
            Position = newPos;
            this.goalTile = goalTile;
            updateCurrentTileIfNeeded();
        }

        private void updateMovement(TimeSpan elapsedTime, Speed currentSpeed)
        {
            IsMoving = true;
            var movementLeft = elapsedTime * currentSpeed;
            while (movementLeft > Unit.Zero)
            {
                var distanceToGoal = (goalPosition - Position).Length;
                if (distanceToGoal > movementLeft)
                {
                    Position += movementLeft * ((goalPosition - Position) / distanceToGoal);
                    break;
                }
                Position = goalPosition;
                movementLeft -= distanceToGoal;
                if (CurrentTile != goalTile)
                {
                    setCurrentTile(goalTile);
                }
                goalTile = goalTile.Neighbour(owner.GetNextDirection());

                // We did not receive a new goal, so unit is standing still.
                if (goalTile != CurrentTile) continue;
                IsMoving = false;
                break;
            }
        }

        private void updateCurrentTileIfNeeded()
        {
            if ((Position - currentTilePosition).LengthSquared <= HexagonInnerRadiusSquared) return;

            var newTile = level.GetTile(Position);
            if (newTile != CurrentTile)
            {
                setCurrentTile(newTile);
            }
        }

        private void setCurrentTile(Tile<TileInfo> newTile)
        {
            var oldTile = CurrentTile;
            CurrentTile = newTile;
            owner.OnTileChanged(oldTile, newTile);
        }
    }

    interface ITileWalkerOwner
    {
        void OnTileChanged(Tile<TileInfo> oldTile, Tile<TileInfo> newTile);
        Direction GetNextDirection();
    }
}
