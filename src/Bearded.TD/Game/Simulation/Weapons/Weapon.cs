﻿using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Game.Simulation.Components.Weapons;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Simulation.Weapons
{
    [ComponentOwner]
    sealed class Weapon : IGameObject, IPositionable, IFactioned, IDirected, IComponentOwner<Weapon>, IListener<CausedDamage>
    {
        private readonly ITurret turret;
        private readonly Building? ownerAsBuilding;

        private readonly ComponentCollection<Weapon> components;
        private readonly ComponentEvents events = new();

        public Maybe<Direction2> AimDirection { get; private set; }
        private Angle currentDirectionOffset;
        public Direction2 CurrentDirection => turret.NeutralDirection + currentDirectionOffset;

        public bool ShootingThisFrame { get; private set; }

        public Direction2 NeutralDirection => turret.NeutralDirection;
        public Maybe<Angle> MaximumTurningAngle => turret.MaximumTurningAngle;

        public Maybe<IComponentOwner> Parent => Just((IComponentOwner)turret.Owner);
        public IGameObject Owner => turret.Owner;
        public Position3 Position => turret.Position;
        public Faction Faction => turret.OwnerFaction;

        public GameState Game => Owner.Game;

        public Weapon(IComponentOwnerBlueprint blueprint, ITurret turret)
        {
            this.turret = turret;
            ownerAsBuilding = turret.Owner as Building;

            components = new ComponentCollection<Weapon>(this, events);
            components.Add(blueprint.GetComponents<Weapon>());
        }

        public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.CanApplyTo(components);

        public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.ApplyTo(components);

        public bool RemoveUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.RemoveFrom(components);

        public void HandleEvent(CausedDamage @event)
        {
            ownerAsBuilding?.AttributeDamage(@event.Target, @event.Result);
        }

        public void AimIn(Direction2 direction)
        {
            AimDirection = Just(direction);
        }

        public void ShootThisFrame()
        {
            ShootingThisFrame = true;
        }

        public void Turn(Angle angle)
        {
            var newDirection = CurrentDirection + angle;
            var newAngleOffset = newDirection - turret.NeutralDirection;

            currentDirectionOffset = MaximumTurningAngle
                .Select(max => newAngleOffset.Clamped(-max, max))
                .ValueOrDefault(newAngleOffset);
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (ownerAsBuilding == null || !ownerAsBuilding.State.IsFunctional)
            {
                return;
            }

            // TODO: component order currently matters - if the order is inverted, the weapon will not shoot
            // this needs a proper solution (event/subscription based system?) to handle communication between components
            // and it has to work no matter what order components are ordered in
            ShootingThisFrame = false;
            AimDirection = Nothing;

            components.Update(elapsedTime);
        }

        public void Draw(CoreDrawers drawers)
        {
            components.Draw(drawers);
        }

        IEnumerable<TComponent> IComponentOwner<Weapon>.GetComponents<TComponent>() => components.Get<TComponent>();

        IEnumerable<T> IComponentOwner.GetComponents<T>() => components.Get<T>();

        Direction2 IDirected.Direction => CurrentDirection;
    }
}
