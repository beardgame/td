using amulware.Graphics;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Factions;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.Weapons;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Components.Generic
{
    interface ITurret : IPositionable
    {
        GameObject Owner { get; }
        Faction OwnerFaction { get; }
        Direction2 NeutralDirection { get; }
    }

    [Component("turret")]
    class Turret<T> : Component<T, ITurretParameters>, ITurret
        where T : BuildingBase<T>
    {
        private Weapon weapon;

        public Position2 Position =>
            Owner.Position + Owner.LocalCoordinateTransform.Transform(Parameters.Offset);

        public Direction2 NeutralDirection => Parameters.NeutralDirection + Owner.LocalOrientationTransform;

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

            var v = NeutralDirection.Vector * geo.LineWidth;
            geo.DrawLine(Position.NumericValue - v, Position.NumericValue + v);
            
            weapon.Draw(geometries);
        }

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
