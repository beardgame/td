using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Synchronization;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Units
{
    [ComponentOwner]
    sealed class EnemyUnit : GameObject,
        IComponentOwner<EnemyUnit>,
        IDamageTarget,
        IPositionable,
        IListener<EnactDeath>,
        IListener<TakeDamage>
    {
        private readonly Id<EnemyUnit> id;
        private readonly IUnitBlueprint blueprint;
        private IEnemyMovement enemyMovement;
        private Unit radius;

        private readonly ComponentEvents events = new();

        private readonly ComponentCollection<EnemyUnit> components;
        private IHealth health = null!;
        private bool isDead;

        public IComponentOwner? Parent => null;

        public Position3 Position => enemyMovement.Position.WithZ(Game.GeometryLayer[CurrentTile].DrawInfo.Height + radius);
        public Tile CurrentTile => enemyMovement.CurrentTile;
        public bool IsMoving => enemyMovement.IsMoving;
        public Circle CollisionCircle => new(Position.XY(), radius);

        private IDamageSource? lastDamageSource;

        public EnemyUnit(Id<EnemyUnit> id, IUnitBlueprint blueprint, Tile currentTile)
        {
            this.id = id;
            this.blueprint = blueprint;

            components = new ComponentCollection<EnemyUnit>(this, events);
            enemyMovement = new EnemyMovementDummy(currentTile);
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            Game.UnitLayer.AddEnemyToTile(CurrentTile, this);

            components.Add(blueprint.GetComponents());
            components.Add(new HealthEventReceiver<EnemyUnit>());
            components.Add(new DamageSource<EnemyUnit>());
            components.Add(new IdProvider<EnemyUnit>(id));
            components.Add(new Syncer<EnemyUnit>());
            components.Add(new TileBasedVisibility<EnemyUnit>());
            health = components.Get<IHealth>().SingleOrDefault()
                ?? throw new InvalidOperationException("All enemies must have a health component.");
            enemyMovement = components.Get<IEnemyMovement>().SingleOrDefault()
                ?? throw new InvalidOperationException("All enemies must have a movement behaviour.");

            radius = ((MathF.Atan(.005f * (health.MaxHealth.NumericValue - 200)) + MathConstants.PiOver2) / MathConstants.Pi * 0.6f).U();
            events.Subscribe<EnactDeath>(this);
            events.Subscribe<TakeDamage>(this);
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
                this.Sync(KillUnit.Command, this, lastDamageSource);
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
            var drawer = drawers.ConsoleBackground;

            drawer.FillCircle(Position.NumericValue, radius.NumericValue, blueprint.Color, 6);

            drawer.FillRectangle(
                Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1, .1f), Color.DarkGray);

            var p = (float) health.HealthPercentage;
            var healthColor = Color.FromHSVA(Interpolate.Lerp(Color.Red.Hue, Color.Green.Hue, p), .8f, .8f);
            drawer.FillRectangle(
                Position.NumericValue - new Vector3(.5f, .5f, 0), new Vector2(1 * p, .1f), healthColor);

            components.Draw(drawers);
        }

        public bool CanApplyEffect(IUpgradeEffect effect) => effect.CanApplyTo(this);

        public void ApplyEffect(IUpgradeEffect effect) => effect.ApplyTo(this);

        public void RemoveEffect(IUpgradeEffect effect) => effect.RemoveFrom(this);

        public void OnTileChanged(Tile oldTile, Tile newTile) =>
            Game.UnitLayer.MoveEnemyBetweenTiles(oldTile, newTile, this);

        public void HandleEvent(TakeDamage @event)
        {
            lastDamageSource = @event.Source ?? lastDamageSource;
        }

        public void HandleEvent(EnactDeath @event)
        {
            isDead = true;
        }

        public void Kill(IDamageSource? damageSource)
        {
            Game.Meta.Events.Send(new EnemyKilled(this, damageSource));
            damageSource?.AttributeKill(this);
            Delete();
        }

        public void AddComponent(IComponent<EnemyUnit> component) => components.Add(component);

        public void RemoveComponent(IComponent<EnemyUnit> component) => components.Remove(component);

        IEnumerable<TComponent> IComponentOwner<EnemyUnit>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<TComponent> IComponentOwner.GetComponents<TComponent>() => components.Get<TComponent>();
    }
}
