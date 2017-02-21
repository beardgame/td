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

        public Position2 Position { get; private set; }
        private Tile<TileInfo> anchorTile;
        protected Tile<TileInfo> CurrentTile { get; private set; }
        private int health;

        protected GameUnit(UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new ArgumentOutOfRangeException();

            Blueprint = blueprint;
            anchorTile = currentTile;
            currentMovementDir = Direction.Unknown;
            health = blueprint.Health;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            updateCurrentTile(anchorTile);
            Position = Game.Level.GetPosition(anchorTile);
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
            Position = Game.Level.GetPosition(anchorTile)
                       + movementProgress * currentMovementDir.SpaceTimeDirection();
        }

        public void Damage(int damage)
        {
            health -= damage;
            OnDamage();
            if (health <= 0) kill();
        }

        private void kill()
        {
            OnKill();
            // boom! <-- almost as good as particle explosions
            Delete();
        }

        private Unit updateMovement(Unit movementLeft)
        {
            var halfwayPoint = .5f * HexagonWidth.U();
            if (movementProgress < halfwayPoint && (movementProgress + movementLeft) >= halfwayPoint)
                updateCurrentTile(anchorTile.Neighbour(currentMovementDir));
            movementProgress += movementLeft;
            if (movementProgress < HexagonWidth.U()) return Unit.Zero;
            anchorTile = CurrentTile;
            currentMovementDir = Direction.Unknown;
            movementProgress = Unit.Zero;
            return movementProgress - HexagonWidth.U();
        }

        private void updateCurrentTile(Tile<TileInfo> newTile)
        {
            var oldTile = CurrentTile;
            CurrentTile = newTile;
            OnTileChange(oldTile, newTile);
        }

        protected abstract Direction GetNextDirection();

        protected virtual void OnTileChange(Tile<TileInfo> oldTile, Tile<TileInfo> newTile)
        { }

        protected virtual void OnDamage()
        { }

        protected virtual void OnKill()
        { }
    }
}
