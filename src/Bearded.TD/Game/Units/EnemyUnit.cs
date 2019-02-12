using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.Units.StatusEffects;
using Bearded.TD.Game.World;
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
        ISyncable<EnemyUnitState>,
        ITileWalkerOwner
    {
        public Id<EnemyUnit> Id { get; }
        
        private readonly IUnitBlueprint blueprint;
        private readonly Tile startTile;
        private TileWalker tileWalker;
        private PassabilityLayer passabilityLayer;
        
        private readonly ComponentCollection<EnemyUnit> components = new ComponentCollection<EnemyUnit>();
        private Health<EnemyUnit> health;

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile CurrentTile => tileWalker?.CurrentTile ?? startTile;
        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        private bool propertiesDirty;
        private EnemyUnitProperties properties;
        private Instant nextAttack;
        private Faction lastDamageSource;
        private readonly List<IStatusEffectSource> statusEffects = new List<IStatusEffectSource>();

        public event GenericEventHandler<int> Damaged;
        public event GenericEventHandler<int> Healed;

        public EnemyUnit(Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile currentTile)
        {
            Id = id;
            this.blueprint = blueprint;
            properties = EnemyUnitProperties.BuilderFromBlueprint(blueprint).Build();
            startTile = currentTile;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);
            
            Game.UnitLayer.AddEnemyToTile(CurrentTile, this);
            tileWalker = new TileWalker(this, Game.Level, startTile);

            passabilityLayer = Game.PassabilityManager.GetLayer(Passability.WalkingUnit);

            nextAttack = Game.Time + properties.TimeBetweenAttacks;
            
            components.Add(this, blueprint.GetComponents());
            health = components.Get<Health<EnemyUnit>>()
                ?? throw new InvalidOperationException("All enemies must have a health component.");
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
            updateStatusEffects(elapsedTime);
            if (propertiesDirty)
            {
                refreshProperties();
            }
            tileWalker.Update(elapsedTime, properties.Speed);
            tryDealDamage();
            components.Update(elapsedTime);
        }

        private void updateStatusEffects(TimeSpan elapsedTime)
        {
            statusEffects.ForEach(effect => effect.Update(elapsedTime));
            var oldSize = statusEffects.Count;
            statusEffects.RemoveAll(effect => effect.HasEnded);
            if (statusEffects.Count < oldSize)
            {
                propertiesDirty = true;
            }
        }

        private void refreshProperties()
        {
            var builder = EnemyUnitProperties.BuilderFromBlueprint(blueprint);
            statusEffects.ForEach(effect => effect.Effect.Apply(builder));
            properties = builder.Build();
            propertiesDirty = false;
        }

        private void tryDealDamage()
        {
            if (tileWalker.IsMoving) return;

            while (nextAttack <= Game.Time)
            {
                var desiredDirection = Game.Navigator.GetDirections(CurrentTile);

                if (!Game.BuildingLayer.TryGetMaterializedBuilding(
                    CurrentTile.Neighbour(desiredDirection), out var target))
                {
                    return;
                }
                
                target.Damage(properties.Damage);
                nextAttack = Game.Time + properties.TimeBetweenAttacks;
            }
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

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            var isPassable = passabilityLayer[CurrentTile.Neighbour(desiredDirection)].IsPassable;
            return !isPassable
                ? Direction.Unknown
                : desiredDirection;
        }

        public void OnTileChanged(Tile oldTile, Tile newTile) =>
            Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, this);

        public void ApplyStatusEffect(IStatusEffectSource statusEffect)
        {
            statusEffects.Add(statusEffect);
            propertiesDirty = true;
        }

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
                tileWalker.GoalTile.X,
                tileWalker.GoalTile.Y,
                health.CurrentHealth,
                properties.Damage,
                properties.TimeBetweenAttacks.NumericValue,
                properties.Speed.NumericValue);
        }

        public void SyncFrom(EnemyUnitState state)
        {
            tileWalker.Teleport(
                new Position2(state.X, state.Y),
                new Tile(state.GoalTileX, state.GoalTileY));
            properties = EnemyUnitProperties.FromState(state);
            
            if (state.Health > health.CurrentHealth) Healed?.Invoke(state.Health - health.CurrentHealth);
            if (state.Health < health.CurrentHealth) Damaged?.Invoke(health.CurrentHealth - state.Health);
        }
    }
}
