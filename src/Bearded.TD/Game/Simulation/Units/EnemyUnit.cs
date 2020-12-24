using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics;
using amulware.Graphics.Shapes;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Damage;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Components.Movement;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Collections;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Units
{
    [ComponentOwner]
    sealed class EnemyUnit : GameObject,
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
        private ImmutableArray<ISyncable> syncables;
        private Health<EnemyUnit> health = null!;
        private bool isDead;

        public Maybe<IComponentOwner> Parent { get; } = Maybe.Nothing;
        public Faction Faction => Game.RootFaction;

        public Unit Radius { get; private set; }

        public Position3 Position => enemyMovement.Position.WithZ(Game.GeometryLayer[CurrentTile].DrawInfo.Height + Radius);
        public Tile CurrentTile => enemyMovement.CurrentTile;
        public bool IsMoving => enemyMovement.IsMoving;
        public Circle CollisionCircle => new Circle(Position.XY(), Radius);
        public long Value => (long) blueprint.Value;

        private IDamageOwner? lastDamageSource;

        public event GenericEventHandler<int>? Healed;

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

            syncables = components.Get<ISyncable>().ToImmutableArray();

            Radius = ((MathF.Atan(.005f * (health.MaxHealth - 200)) + MathConstants.PiOver2) / MathConstants.Pi * 0.6f).U();
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
                this.Sync(KillUnit.Command, this, lastDamageSource?.Faction);
            }
        }

        public override void Draw(GeometryManager geometries)
        {
            var drawer = geometries.ConsoleBackground;

            drawer.FillCircle(Position.NumericValue, Radius.NumericValue, blueprint.Color, 6);

            drawer.FillRectangle(
                Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1, .1f), Color.DarkGray);

            var p = (float) health.HealthPercentage;
            var healthColor = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            drawer.FillRectangle(
                Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1 * p, .1f), healthColor);

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

        public void Execute(Faction? killingBlowFaction)
        {
            Game.Meta.Events.Send(new EnemyKilled(this, killingBlowFaction));
            Delete();
        }

        IEnumerable<TComponent> IComponentOwner<EnemyUnit>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();

        public IStateToSync GetCurrentStateToSync() => new CompositeStateToSync(syncables.Select(s => s.GetCurrentStateToSync()));
    }
}
