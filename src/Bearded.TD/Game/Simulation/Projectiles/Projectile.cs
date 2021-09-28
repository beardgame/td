﻿using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
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

        public Position3 Position { get; set; }

        public Projectile(
            IComponentOwnerBlueprint blueprint, Position3 position, Velocity3 velocity, Building? damageSource)
        {
            this.blueprint = blueprint;
            upgradeManager = damageSource?.GetComponents<IBuildingUpgradeManager>().SingleOrDefault();
            Position = position;
            DamageSource = damageSource;

            components = new ComponentCollection<Projectile>(this, events);
            events.Subscribe(this);

            components.Add(new ParabolicMovement(velocity));
        }

        protected override void OnAdded()
        {
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
            components.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        public void AddComponent(IComponent<Projectile> component) => components.Add(component);

        IEnumerable<TComponent> IComponentOwner<Projectile>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();
    }
}
