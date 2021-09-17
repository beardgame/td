using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [ComponentOwner]
    sealed class Projectile : GameObject, IPositionable, IComponentOwner<Projectile>, IListener<CausedDamage>
    {
        private readonly IComponentOwnerBlueprint blueprint;
        private readonly IBuildingUpgradeManager? upgradeManager;
        private readonly ComponentCollection<Projectile> components;
        private readonly ComponentEvents events = new();

        public Building? DamageSource { get; }

        public Maybe<IComponentOwner> Parent => Maybe.FromNullable<IComponentOwner>(DamageSource);

        public Position3 Position { get; private set; }

        public Velocity3 Velocity { get; private set; }

        public Tile CurrentTile { get; private set; }

        public Projectile(
            IComponentOwnerBlueprint blueprint, Position3 position, Velocity3 velocity, Building? damageSource)
        {
            this.blueprint = blueprint;
            upgradeManager = damageSource?.GetComponents<IBuildingUpgradeManager>().SingleOrDefault();
            Position = position;
            Velocity = velocity;
            DamageSource = damageSource;

            components = new ComponentCollection<Projectile>(this, events);
            events.Subscribe(this);
        }

        protected override void OnAdded()
        {
            CurrentTile = Level.GetTile(Position.XY());

            components.Add(blueprint.GetComponents<Projectile>());

            foreach (var upgrade in upgradeManager?.AppliedUpgrades ?? Enumerable.Empty<IUpgradeBlueprint>())
            {
                if (!upgrade.CanApplyTo(this))
                {
                    continue;
                }

                upgrade.ApplyTo(this);
            }
        }

        public void HandleEvent(CausedDamage @event)
        {
            DamageSource?.AttributeDamage(@event.Target, @event.Result);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var forces = Constants.Game.Physics.Gravity3;

            Velocity +=  forces * elapsedTime;

            var step = Velocity * elapsedTime;
            var ray = new Ray(Position.XY(), step.XY());

            var (result, rayFactor, _, enemy) = Game.Level.CastRayAgainstEnemies(
                ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile));

            Position += step * rayFactor;

            switch (result)
            {
                case RayCastResultType.HitNothing:
                    if (Position.Z < Game.GeometryLayer[CurrentTile].DrawInfo.Height)
                    {
                        events.Send(new HitLevel());
                        Delete();
                    }
                    break;
                case RayCastResultType.HitLevel:
                    events.Send(new HitLevel());
                    Delete();
                    break;
                case RayCastResultType.HitEnemy:
                    enemy.Match(e => events.Send(new HitEnemy(e)), () => throw new InvalidOperationException());
                    Delete();
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            if (Deleted)
            {
                return;
            }

            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);

            if (!UserSettings.Instance.Debug.ProjectileDots)
            {
                return;
            }

            drawers.Primitives.FillRectangle(
                Position.NumericValue - new Vector3(0.1f, 0.1f, 0f), new Vector2(0.2f, 0.2f), Color.Yellow);
        }

        public void AddComponent(IComponent<Projectile> component) => components.Add(component);

        IEnumerable<TComponent> IComponentOwner<Projectile>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}
