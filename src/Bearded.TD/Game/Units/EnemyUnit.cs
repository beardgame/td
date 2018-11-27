using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
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

namespace Bearded.TD.Game.Units
{
    class EnemyUnit : GameObject, IIdable<EnemyUnit>, IPositionable, ISyncable<EnemyUnitState>, ITileWalkerOwner
    {
        public Id<EnemyUnit> Id { get; }
        
        private readonly IUnitBlueprint blueprint;
        private readonly Tile startTile;
        private TileWalker tileWalker;

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile CurrentTile => tileWalker?.CurrentTile ?? startTile;
        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        private bool propertiesDirty;
        private EnemyUnitProperties properties;
        private int health;
        private Instant nextAttack;
        private readonly List<IStatusEffectSource> statusEffects = new List<IStatusEffectSource>();

        public EnemyUnit(Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile currentTile)
        {
            if (!Game.Level.IsValid(currentTile)) throw new System.ArgumentOutOfRangeException();

            Id = id;
            this.blueprint = blueprint;
            properties = EnemyUnitProperties.BuilderFromBlueprint(blueprint).Build();
            startTile = currentTile;
            health = blueprint.Health;
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);

            tileWalker = new TileWalker(this, Game.Level);
            tileWalker.Teleport(Game.Level.GetPosition(startTile), startTile);
            Game.UnitLayer.AddEnemyToTile(CurrentTile, this);

            nextAttack = Game.Time + properties.TimeBetweenAttacks;
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            Game.UnitLayer.RemoveEnemyFromTile(CurrentTile, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            updateStatusEffects(elapsedTime);
            if (propertiesDirty)
            {
                refreshProperties();
            }
            tileWalker.Update(elapsedTime, properties.Speed);
            tryDealDamage();
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
            var size = (Mathf.Atan(.005f * (blueprint.Health - 200)) + Mathf.PiOver2) / Mathf.Pi * 0.6f;
            geo.DrawCircle(Position.NumericValue, size, true, 6);

            var p = (health / (float)blueprint.Health).Clamped(0, 1);
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1, .1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1 * p, .1f));

            geometries.PointLight.Draw(
                Position.NumericValue.WithZ(0.5f),
                1.5f,
                blueprint.Color
                );
        }

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            return !CurrentTile.Neighbour(desiredDirection).Info.IsPassableFor(TileInfo.PassabilityLayer.Unit)
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
            health -= damage;
            if (health <= 0)
                this.Sync(KillUnit.Command, this, damageSource.Faction);
        }

        public void Kill(Faction killingBlowFaction)
        {
            // killingBlowFaction?.Resources.ProvideOneTimeResource(blueprint.Value);
            // boom! <-- almost as good as particle explosions
            Delete();
        }

        public EnemyUnitState GetCurrentState()
        {
            return new EnemyUnitState(
                Position.X.NumericValue,
                Position.Y.NumericValue,
                tileWalker.GoalTile.X,
                tileWalker.GoalTile.Y,
                health,
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
            health = state.Health;
        }
    }
}
