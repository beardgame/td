using System;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Units
{
    abstract class GameUnit : GameObject
    {
        protected UnitBlueprint Blueprint { get; }
        private Direction currentMovementDir;
        private Unit movementProgress;

        protected Position2 Position { get; private set; }
        protected Tile<TileInfo> CurrentTile { get; private set; }
        private int health;

        protected GameUnit(UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new ArgumentOutOfRangeException();

            this.Blueprint = blueprint;
            CurrentTile = currentTile;
            currentMovementDir = Direction.Unknown;
            health = blueprint.Health;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Position = Game.Level.GetPosition(CurrentTile);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var movementLeft = elapsedTime * Blueprint.Speed;
            while (movementLeft > Unit.Zero)
            {
                if (currentMovementDir == Direction.Unknown)
                {
                    currentMovementDir = GetNextDirection();
                    if (currentMovementDir == Direction.Unknown) break;
                }

                movementLeft = updateMovement(movementLeft);
            }
            Position = Game.Level.GetPosition(CurrentTile)
                       + movementProgress * currentMovementDir.SpaceTimeDirection();
        }

        private Unit updateMovement(Unit movementLeft)
        {
            movementProgress += movementLeft;
            if (movementProgress < HexagonWidth.U()) return Unit.Zero;
            CurrentTile = CurrentTile.Neighbour(currentMovementDir);
            currentMovementDir = Direction.Unknown;
            movementProgress = Unit.Zero;
            return movementProgress - HexagonWidth.U();
        }

        protected abstract Direction GetNextDirection();
    }
}
