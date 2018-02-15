using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.Units.StatusEffects;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
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
        
        private readonly UnitBlueprint blueprint;
        private readonly Tile<TileInfo> startTile;
        private TileWalker tileWalker;

        public Position2 Position => tileWalker?.Position ?? Game.Level.GetPosition(CurrentTile);
        public Tile<TileInfo> CurrentTile => tileWalker?.CurrentTile ?? startTile;
        public Circle CollisionCircle => new Circle(Position, HexagonSide.U() * 0.5f);

        private bool propertiesDirty;
        private EnemyUnitProperties properties;
        private int health;
        private Instant nextAttack;
        private readonly List<IStatusEffectSource> statusEffects = new List<IStatusEffectSource>();

        public EnemyUnit(Id<EnemyUnit> id, UnitBlueprint blueprint, Tile<TileInfo> currentTile)
        {
            if (!currentTile.IsValid) throw new System.ArgumentOutOfRangeException();

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

            nextAttack = Game.Time + properties.TimeBetweenAttacks;
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            CurrentTile.Info.RemoveEnemy(this);
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
                var target = CurrentTile.Neighbour(desiredDirection).Info.FinishedBuilding;

                if (target == null) return;
                
                target.Damage(properties.Damage);
                nextAttack = Game.Time + properties.TimeBetweenAttacks;
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = blueprint.Color;
            var size = (Mathf.Atan(.005f * (blueprint.Health - 200)) + Mathf.PiOver2) / Mathf.Pi;
            geo.DrawRectangle(Position.NumericValue - Vector2.One * size * .5f, Vector2.One * size);

            var p = (health / (float)blueprint.Health).Clamped(0, 1);
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1, .1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            geo.DrawRectangle(Position.NumericValue - new Vector2(.5f), new Vector2(1 * p, .1f));
        }

        public Direction GetNextDirection()
        {
            var desiredDirection = Game.Navigator.GetDirections(CurrentTile);
            return !CurrentTile.Neighbour(desiredDirection).Info.IsPassableFor(TileInfo.PassabilityLayer.Unit)
                ? Direction.Unknown
                : desiredDirection;
        }

        public void OnTileChanged(Tile<TileInfo> oldTile, Tile<TileInfo> newTile)
        {
            if (oldTile.IsValid)
                oldTile.Info.RemoveEnemy(this);
            if (newTile.IsValid)
                newTile.Info.AddEnemy(this);
        }

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
            killingBlowFaction?.Resources.ProvideOneTimeResource(blueprint.Value);
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
                new Tile<TileInfo>(Game.Level.Tilemap, state.GoalTileX, state.GoalTileY));
            properties = EnemyUnitProperties.FromState(state);
            health = state.Health;
        }
    }
}
