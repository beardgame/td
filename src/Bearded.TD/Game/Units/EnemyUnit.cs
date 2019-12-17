using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Damage;
using Bearded.TD.Game.Components.EnemyBehavior;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Components.Movement;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
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
        ISyncable,
        IDamageOwner
    {
        public Id<EnemyUnit> Id { get; }

        private readonly IUnitBlueprint blueprint;
        private IEnemyMovement enemyMovement;

        private readonly ComponentEvents events = new ComponentEvents();

        private readonly ComponentCollection<EnemyUnit> components;
        private ImmutableList<ISyncable> syncables;
        private Health<EnemyUnit> health;
        private bool isDead;


        public Maybe<IComponentOwner> Parent { get; } = Maybe.Nothing;
        public Faction Faction => Game.RootFaction;

        public Unit Radius { get; private set; }

        public Position3 Position => enemyMovement.Position.WithZ(Game.GeometryLayer[CurrentTile].DrawInfo.Height + Radius);
        public Tile CurrentTile => enemyMovement.CurrentTile;
        public bool IsMoving => enemyMovement.IsMoving;
        public Circle CollisionCircle => new Circle(Position.XY(), Radius);
        public long Value => (long) blueprint.Value;

        private IDamageOwner lastDamageSource;

        public event GenericEventHandler<int> Healed;

        public EnemyUnit(Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile currentTile)
        {
            Id = id;
            this.blueprint = blueprint;

            components = new ComponentCollection<EnemyUnit>(this, events);
            enemyMovement = new EnemyMovementDummy(currentTile);
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.IdAs(this);
            Game.Meta.Synchronizer.RegisterSyncable(this);

            Game.UnitLayer.AddEnemyToTile(CurrentTile, this);

            components.Add(blueprint.GetComponents());
            health = components.Get<Health<EnemyUnit>>().SingleOrDefault()
                ?? throw new InvalidOperationException("All enemies must have a health component.");
            enemyMovement = components.Get<IEnemyMovement>().SingleOrDefault()
                ?? throw new InvalidOperationException("All enemies must have a movement behaviour.");

            syncables = components.Get<ISyncable>().ToImmutableList();

            Radius = ((Mathf.Atan(.005f * (health.MaxHealth - 200)) + Mathf.PiOver2) / Mathf.Pi * 0.6f).U();
        }

        protected override void OnDelete()
        {
            base.OnDelete();
            Game.UnitLayer.RemoveEnemyFromTile(CurrentTile, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            components.Update(elapsedTime);

            if (isDead)
            {
                this.Sync(KillUnit.Command, this, lastDamageSource.Faction);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = blueprint.Color;
            geo.DrawCircle(Position.NumericValue, Radius.NumericValue, true, 6);

            var p = (float) health.HealthPercentage;
            geo.Color = Color.DarkGray;
            geo.DrawRectangle(Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1, .1f));
            geo.Color = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            geo.DrawRectangle(Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1 * p, .1f));

            components.Draw(geometries);
        }

        public bool CanApplyEffect(IUpgradeEffect effect) => effect.CanApplyTo(components);

        public void ApplyEffect(IUpgradeEffect effect) => effect.ApplyTo(components);

        public void RemoveEffect(IUpgradeEffect effect) => effect.RemoveFrom(components);

        public void OnTileChanged(Tile oldTile, Tile newTile) =>
            Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, this);

        public void Damage(DamageInfo damageInfo)
        {
            lastDamageSource = damageInfo.Source;
            events.Send(new TakeDamage(damageInfo));
        }

        public void OnDeath()
        {
            isDead = true;
        }

        public void Execute(Faction killingBlowFaction)
        {
            Game.Meta.Events.Send(new EnemyKilled(this, killingBlowFaction));
            Delete();
        }

        IEnumerable<TComponent> IComponentOwner<EnemyUnit>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();

        public IStateToSync GetCurrentStateToSync() => new CompositeStateToSync(syncables.Select(s => s.GetCurrentStateToSync()));
    }
}
