using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
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

        public Position2 Position { get; private set; }
        protected int Health { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }
        private Position2 currentTilePosition => Game.Level.GetPosition(CurrentTile);
        private Tile<TileInfo> goalTile;
        private Position2 goalPosition => Game.Level.GetPosition(goalTile);
        protected bool IsMoving { get; private set; }

        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        protected GameUnit(Id<GameUnit> id, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new ArgumentOutOfRangeException();

            Id = id;
            Blueprint = blueprint;
            CurrentTile = currentTile;
            goalTile = currentTile;
            Health = blueprint.Health;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);

            Position = currentTilePosition;
            setCurrentTile(goalTile);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            updateMovement(elapsedTime);
            updateCurrentTileIfNeeded();
        }

        private void updateMovement(TimeSpan elapsedTime)
        {
            IsMoving = true;
            var movementLeft = elapsedTime * Blueprint.Speed;
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
                    setCurrentTile(goalTile);
                goalTile = goalTile.Neighbour(GetNextDirection());

                // We did not receive a new goal, so unit is standing still.
                if (goalTile != CurrentTile) continue;
                IsMoving = false;
                break;
            }
        }

        private void updateCurrentTileIfNeeded()
        {
            if ((Position - currentTilePosition).LengthSquared <= HexagonInnerRadiusSquared) return;

            var newTile = Game.Level.GetTile(Position);
            if (newTile != CurrentTile)
            {
                setCurrentTile(newTile);
            }
        }

        private void setCurrentTile(Tile<TileInfo> newTile)
        {
            var oldTile = CurrentTile;
            CurrentTile = newTile;
            OnTileChange(oldTile, newTile);
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

        protected abstract Direction GetNextDirection();

        protected virtual void OnTileChange(Tile<TileInfo> oldTile, Tile<TileInfo> newTile)
        { }

        protected virtual void OnDamage()
        { }

        protected virtual void OnKill(Faction killingBlowFaction)
        { }

        public GameUnitState GetCurrentState()
        {
            return new GameUnitState(Position.X.NumericValue, Position.Y.NumericValue, Health);
        }

        public void SyncFrom(GameUnitState state)
        {
            Position = new Position2(state.X, state.Y);
            Health = state.Health;
            updateCurrentTileIfNeeded();
        }
    }
}
