﻿using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Components.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Game.Weapons
{
    [ComponentOwner]
    class Weapon : IPositionable, IFactioned
    {
        private readonly ITurret turret;
        private readonly Building ownerAsBuilding;

        private readonly ComponentCollection<Weapon> components = new ComponentCollection<Weapon>();

        public Maybe<Direction2> AimDirection { get; private set; }
        private Angle currentDirectionOffset;
        public Direction2 CurrentDirection => turret.NeutralDirection + currentDirectionOffset;
        public bool ShootingThisFrame { get; private set; }

        public GameObject Owner => turret.Owner;
        public Position2 Position => turret.Position;
        public Faction Faction => turret.OwnerFaction;

        public Weapon(IWeaponBlueprint blueprint, ITurret turret)
        {
            this.turret = turret;
            ownerAsBuilding = turret.Owner as Building;

            components.Add(this, blueprint.GetComponents());
        }

        public bool CanApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.CanApplyTo(components);

        public void ApplyUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.ApplyTo(components);

        public bool RemoveUpgradeEffect(IUpgradeEffect upgradeEffect) => upgradeEffect.RemoveFrom(components);


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

            currentDirectionOffset = turret.MaximumTurningAngle
                .Select(max => newAngleOffset.Clamped(-max, max))
                .ValueOrDefault(newAngleOffset);
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (ownerAsBuilding == null || !ownerAsBuilding.IsCompleted)
                return;

            // TODO: component order currently matters - if the order is inverted, the weapon will not shoot
            // this needs a proper solution (event/subscription based system?) to handle communication between components
            // and it has to work no matter what order components are ordered in
            ShootingThisFrame = false;
            AimDirection = Nothing;

            components.Update(elapsedTime);
        }

        public void Draw(GeometryManager geometries)
        {
            var geo = geometries.Primitives;
            geo.Color = Color.Red;
            geo.LineWidth = 0.15f;

            var v = CurrentDirection.Vector * geo.LineWidth;
            geo.DrawLine(Position.NumericValue - v, Position.NumericValue + v * 2);

            components.Draw(geometries);
        }
    }
}
