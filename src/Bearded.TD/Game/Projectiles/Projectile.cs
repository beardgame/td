using System;
using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Projectiles
{
    [ComponentOwner]
    class Projectile : GameObject, IPositionable, IComponentEventManager, IComponentOwner<Projectile>
    {
        public Building DamageSource { get; }

        private readonly IComponentOwnerBlueprint blueprint;
        private readonly ComponentCollection<Projectile> components;

        public Maybe<IComponentOwner> Parent => Maybe.Just<IComponentOwner>(DamageSource);

        public ComponentEvents Events { get; } = new ComponentEvents();

        public Position3 Position { get; private set; }

        public Velocity3 Velocity { get; private set; }

        public Tile CurrentTile { get; private set; }

        public Projectile(IComponentOwnerBlueprint blueprint, Position3 position, Velocity3 velocity, Building damageSource)
        {
            this.blueprint = blueprint;
            DamageSource = damageSource;
            Position = position;
            Velocity = velocity;

            components = new ComponentCollection<Projectile>(this);
        }

        protected override void OnAdded()
        {
            CurrentTile = Level.GetTile(Position.XY());

            components.Add(blueprint.GetComponents<Projectile>());

            foreach (var upgrade in DamageSource.AppliedUpgrades)
            {
                if (!upgrade.CanApplyTo(components))
                    continue;

                upgrade.ApplyTo(components);
            }
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
                case RayCastResult.HitNothing:
                    if (Position.Z < Game.GeometryLayer[CurrentTile].DrawInfo.Height)
                    {
                        Events.Send(new HitLevel());
                        Delete();
                    }
                    break;
                case RayCastResult.HitLevel:
                    Events.Send(new HitLevel());
                    Delete();
                    break;
                case RayCastResult.HitEnemy:
                    Events.Send(new HitEnemy(enemy));
                    Delete();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Deleted)
                return;

            components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            components.Draw(geometries);

            if (!UserSettings.Instance.Debug.ProjectileDots)
                return;

            var geo = geometries.Primitives;
            geo.Color = Color.Yellow;
            geo.DrawRectangle(Position.NumericValue - new Vector3(0.1f, 0.1f, 0f), new Vector2(0.2f, 0.2f));
        }

        IEnumerable<TComponent> IComponentOwner<Projectile>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}
