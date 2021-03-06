﻿using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Weapons
{
    interface ITurret : IPositionable
    {
        Weapon Weapon { get; }
        IGameObject Owner { get; }
        Faction OwnerFaction { get; }
        Direction2 NeutralDirection { get; }
        Maybe<Angle> MaximumTurningAngle { get; }
    }

    [Component("turret")]
    sealed class Turret<T> : Component<T, ITurretParameters>, ITurret
        where T : IFactioned, IGameObject, IPositionable, ITransformable
    {
        private Weapon weapon = null!;

        public Position3 Position =>
            (Owner.Position.XY() + Owner.LocalCoordinateTransform.Transform(Parameters.Offset))
            .WithZ(Owner.Position.Z + Parameters.Height);

        public Direction2 NeutralDirection => Parameters.NeutralDirection + Owner.LocalOrientationTransform;
        public Maybe<Angle> MaximumTurningAngle => Maybe.FromNullable(Parameters.MaximumTurningAngle);

        public Turret(ITurretParameters parameters) : base(parameters) { }

        protected override void Initialize()
        {
            weapon = new Weapon(Parameters.Weapon, this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            weapon.Update(elapsedTime);
        }

        public override void Draw(CoreDrawers drawers)
        {
            weapon.Draw(drawers);
        }

        Weapon ITurret.Weapon => weapon;
        IGameObject ITurret.Owner => Owner;
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
