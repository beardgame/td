using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IProjectileEmitterParameters : IParametersTemplate<IProjectileEmitterParameters>
    {
        ProjectileBlueprint Projectile { get; }

        [Modifiable(.15, Type = AttributeType.Cooldown)]
        TimeSpan ShootInterval { get; }

        [Modifiable(10.0)]
        Speed MuzzleVelocity { get; }
    }
}
