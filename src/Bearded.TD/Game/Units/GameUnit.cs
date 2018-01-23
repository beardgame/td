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
    abstract class GameUnit : GameObject, IIdable<GameUnit>, ISyncable<GameUnitState>, ITileMoverOwner
    {
        public Id<GameUnit> Id { get; }
        protected UnitBlueprint Blueprint { get; }

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        protected int Health { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }

        private TileWalker tileWalker;

        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        protected bool IsMoving => tileWalker.IsMoving;

        protected GameUnit(Id<GameUnit> id, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new ArgumentOutOfRangeException();

            Id = id;
            Blueprint = blueprint;
            CurrentTile = currentTile;
            Health = blueprint.Health;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            tileWalker.Update(elapsedTime);
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);
            
            tileWalker = new TileWalker(this, Game.Level, Blueprint.Speed);
            tileWalker.Teleport(Game.Level.GetPosition(CurrentTile), CurrentTile);
        }
        
        public void UpdateTile(Tile<TileInfo> newTile)
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

        public abstract Direction GetNextDirection();

        protected virtual void OnTileChange(Tile<TileInfo> oldTile, Tile<TileInfo> newTile)
        { }

        protected virtual void OnDamage()
        { }

        protected virtual void OnKill(Faction killingBlowFaction)
        { }

        public GameUnitState GetCurrentState()
        {
            return new GameUnitState(
                Position.X.NumericValue,
                Position.Y.NumericValue,
                tileWalker.GoalTile.X,
                tileWalker.GoalTile.Y,
                Health);
        }

        public void SyncFrom(GameUnitState state)
        {
            tileWalker.Teleport(
                new Position2(state.X, state.Y),
                new Tile<TileInfo>(Game.Level.Tilemap, state.GoalTileX, state.GoalTileY));
            Health = state.Health;
        }
    }
}
