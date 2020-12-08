﻿using Bearded.TD.Content.Models;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Game.GameState.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Components.Weapons
{
    interface ITurret : IPositionable
    {
        Weapon Weapon { get; }
        GameObject Owner { get; }
        Faction OwnerFaction { get; }
        Direction2 NeutralDirection { get; }
        Maybe<Angle> MaximumTurningAngle { get; }
    }

    [Component("turret")]
    class Turret<T> : Component<T, ITurretParameters>, ITurret
        where T : BuildingBase<T>, IComponentOwner
    {
        private Weapon weapon;

        public Position3 Position =>
            (Owner.Position.XY() + Owner.LocalCoordinateTransform.Transform(Parameters.Offset))
            .WithZ(Owner.Position.Z + Parameters.Height);

        public Direction2 NeutralDirection => Parameters.NeutralDirection + Owner.LocalOrientationTransform;
        public Maybe<Angle> MaximumTurningAngle => Maybe.FromNullable(Parameters.MaximumTurningAngle);

        public Turret(ITurretParameters parameters) : base(parameters) { }

        protected override void Initialise()
        {
            weapon = new Weapon(Parameters.Weapon, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            weapon.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            const float lineWidth = .2f;

            var v = (NeutralDirection.Vector * lineWidth).WithZ();
            // geometries.Primitives.DrawLine(
            //     Position.NumericValue - v, Position.NumericValue + v, lineWidth, Color.Green);

            weapon.Draw(geometries);
        }

        Weapon ITurret.Weapon => weapon;
        GameObject ITurret.Owner => Owner;
        Faction ITurret.OwnerFaction => Owner.Faction;

        public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
        {
            return base.CanApplyUpgradeEffect(effect) || weapon.CanApplyUpgradeEffect(effect);
        }

        public override void ApplyUpgradeEffect(IUpgradeEffect effect)
        {
            base.ApplyUpgradeEffect(effect);
            weapon.ApplyUpgradeEffect(effect);
        }

        public override bool RemoveUpgradeEffect(IUpgradeEffect effect)
        {
            var removed = false;
            removed |= base.RemoveUpgradeEffect(effect);
            removed |= weapon.RemoveUpgradeEffect(effect);
            return removed;
        }
    }
}