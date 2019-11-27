using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IProjectileEmitterParameters : IParametersTemplate<IProjectileEmitterParameters>
    {
        IComponentOwnerBlueprint Projectile { get; }

        [Modifiable(6.6, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }

        // TODO: This should be muzzleSPEED
        [Modifiable(10.0)]
        Speed MuzzleVelocity { get; }

        [Modifiable(0.0, Type = AttributeType.SpreadAngle)]
        Angle Spread { get; }
    }
}
