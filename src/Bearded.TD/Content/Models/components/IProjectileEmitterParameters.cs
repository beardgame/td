using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IProjectileEmitterParameters : IParametersTemplate<IProjectileEmitterParameters>
    {
        ProjectileBlueprint Projectile { get; }

        [Modifiable(6.6, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }

        [Modifiable(10.0)]
        Speed MuzzleVelocity { get; }
    }
}
