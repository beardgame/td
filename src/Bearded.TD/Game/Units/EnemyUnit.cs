using System;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.EnemyBehavior;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Units
{
    [ComponentOwner]
    class EnemyUnit : GameObject,
        IComponentOwner<EnemyUnit>,
        IIdable<EnemyUnit>,
        IMortal,
        IPositionable,
        ISyncable<EnemyUnitState>
    {
        public Id<EnemyUnit> Id { get; }
        
        private readonly IUnitBlueprint blueprint;
        private readonly Tile startTile;
        private IEnemyMovement enemyMovement;
        
        private readonly ComponentCollection<EnemyUnit> components = new ComponentCollection<EnemyUnit>();
        private Health<EnemyUnit> health;

        public Position2 Position => enemyMovement?.Position ?? Level.GetPosition(CurrentTile);
        public Tile CurrentTile => enemyMovement?.CurrentTile ?? startTile;
        public bool IsMoving => enemyMovement?.IsMoving ?? false;
        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        private Faction lastDamageSource;

        public event GenericEventHandler<int> Damaged;
        public event GenericEventHandler<int> Healed;

        public EnemyUnit(Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile currentTile)
        {
            Id = id;
            this.blueprint = blueprint;
            startTile = currentTile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);
            
            Game.UnitLayer.AddEnemyToTile(CurrentTile, this);
            
            components.Add(this, blueprint.GetComponents());
            health = components.Get<Health<EnemyUnit>>()
                ?? throw new InvalidOperationException("All enemies must have a health component.");
            enemyMovement = components.Get<IEnemyMovement>()
                ?? throw new InvalidOperationException("All enemies must have a movement behaviour.");
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            Game.UnitLayer.RemoveEnemyFromTile(CurrentTile, this);
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent<EnemyUnit>
            => components.Get<TComponent>();

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = blueprint.Color;
            var size = (Mathf.Atan(.005f * (health.MaxHealth - 200)) + Mathf.PiOver2) / Mathf.Pi * 0.6f;
            geo.DrawCircle(Position.NumericValue, size, true, 6);

            var p = (float) health.HealthPercentage;
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1, .1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1 * p, .1f));

            geometries.PointLight.Draw(
                Position.NumericValue.WithZ(0.5f),
                1.5f,
                blueprint.Color
                );
            
            components.Draw(geometries);
        }

        public bool CanApplyEffect(IUpgradeEffect effect) => effect.CanApplyTo(components);

        public void ApplyEffect(IUpgradeEffect effect) => effect.ApplyTo(components);

        public void RemoveEffect(IUpgradeEffect effect) => effect.RemoveFrom(components);

        public void OnTileChanged(Tile oldTile, Tile newTile) =>
            Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, this);

        public void Damage(int damage, Building damageSource)
        {
            lastDamageSource = damageSource.Faction;
            Damaged?.Invoke(damage);
        }
        
        public void OnDeath()
        {
            this.Sync(KillUnit.Command, this, lastDamageSource);
        }

        public void Kill(Faction killingBlowFaction)
        {
            Delete();
        }

        public EnemyUnitState GetCurrentState()
        {
            return new EnemyUnitState(
                Position.X.NumericValue,
                Position.Y.NumericValue,
                enemyMovement.GoalTile.X,
                enemyMovement.GoalTile.Y,
                health.CurrentHealth);
        }

        public void SyncFrom(EnemyUnitState state)
        {
            enemyMovement.Teleport(
                new Position2(state.X, state.Y),
                new Tile(state.GoalTileX, state.GoalTileY));
            
            if (state.Health > health.CurrentHealth) Healed?.Invoke(state.Health - health.CurrentHealth);
            if (state.Health < health.CurrentHealth) Damaged?.Invoke(health.CurrentHealth - state.Health);
        }
    }
}
