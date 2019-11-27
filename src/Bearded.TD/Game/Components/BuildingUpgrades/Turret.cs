﻿using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.BuildingUpgrades
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
        where T : BuildingBase<T>
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
            var geo = geometries.Primitives;
            geo.Color = Color.Green;
            geo.LineWidth = 0.2f;

            var v = (NeutralDirection.Vector * geo.LineWidth).WithZ();
            geo.DrawLine(Position.NumericValue - v, Position.NumericValue + v);

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
