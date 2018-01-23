using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.World
{
    sealed class TileWalker
    {
        private readonly ITileWalkerOwner owner;
        private readonly Level level;
        private readonly Speed movementSpeed;

        public Position2 Position { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }
        private Position2 currentTilePosition => level.GetPosition(CurrentTile);
        private Tile<TileInfo> goalTile;
        private Position2 goalPosition => level.GetPosition(goalTile);

        public Tile<TileInfo> GoalTile => goalTile;
        public bool IsMoving { get; private set; }

        public TileWalker(ITileWalkerOwner owner, Level level, Speed movementSpeed)
        {
            this.owner = owner;
            this.level = level;
            this.movementSpeed = movementSpeed;
        }

        public void Update(TimeSpan elapsedTime)
        {
            updateMovement(elapsedTime);
            updateCurrentTileIfNeeded();
        }

        public void Teleport(Position2 newPos, Tile<TileInfo> goalTile)
        {
            Position = newPos;
            this.goalTile = goalTile;
            updateCurrentTileIfNeeded();
        }

        private void updateMovement(TimeSpan elapsedTime)
        {
            IsMoving = true;
            var movementLeft = elapsedTime * movementSpeed;
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
