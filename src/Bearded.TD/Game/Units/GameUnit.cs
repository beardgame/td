using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Synchronization;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Units
{
    abstract class GameUnit : GameObject, IIdable<GameUnit>, ISyncable<GameUnitState>
    {
        public Id<GameUnit> Id { get; }
        protected UnitBlueprint Blueprint { get; }
        protected Direction CurrentMovementDirection { get; private set; }
        private Unit movementProgress;

        public Position2 Position { get; private set; }
        private Tile<TileInfo> anchorTile;
        protected Tile<TileInfo> CurrentTile { get; private set; }
        protected int Health { get; private set; }

        protected GameUnit(Id<GameUnit> id, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new ArgumentOutOfRangeException();

            Id = id;
            Blueprint = blueprint;
            anchorTile = currentTile;
            CurrentMovementDirection = Direction.Unknown;
            Health = blueprint.Health;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);

            updateCurrentTile(anchorTile);
            Position = Game.Level.GetPosition(anchorTile);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var movementLeft = elapsedTime * Blueprint.Speed;
            while (movementLeft > Unit.Zero)
            {
                if (CurrentMovementDirection == Direction.Unknown)
                {
                    CurrentMovementDirection = GetNextDirection();
                    if (CurrentMovementDirection == Direction.Unknown) break;
                }

                movementLeft = updateMovement(movementLeft);
            }
            Position = Game.Level.GetPosition(anchorTile)
                       + movementProgress * CurrentMovementDirection.SpaceTimeDirection();
        }

        public void Damage(int damage, Building damageSource)
        {
            Health -= damage;
            OnDamage();
            if (Health <= 0)
                this.Sync(KillUnit.Command, this, damageSource.Faction);
        }

        public void Kill(Faction killingBlowFaction)
        {
            OnKill(killingBlowFaction);
            // boom! <-- almost as good as particle explosions
            Delete();
        }

        private Unit updateMovement(Unit movementLeft)
        {
            var halfwayPoint = .5f * HexagonWidth.U();
            if (movementProgress < halfwayPoint && (movementProgress + movementLeft) >= halfwayPoint)
                updateCurrentTile(anchorTile.Neighbour(CurrentMovementDirection));
            movementProgress += movementLeft;
            if (movementProgress < HexagonWidth.U()) return Unit.Zero;
            anchorTile = CurrentTile;
            CurrentMovementDirection = Direction.Unknown;
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

        protected virtual void OnKill(Faction killingBlowFaction)
        { }

        public GameUnitState GetCurrentState()
        {
            return new GameUnitState(
                anchorTile.X, anchorTile.Y, (byte) CurrentMovementDirection, movementProgress.NumericValue, Health);
        }

        public void SyncFrom(GameUnitState state)
        {
            anchorTile = new Tile<TileInfo>(Game.Level.Tilemap, state.TileX, state.TileY);
            CurrentMovementDirection = (Direction) state.Direction;
            movementProgress = new Unit(state.MovementProgress);
            Health = state.Health;
        }
    }
}
