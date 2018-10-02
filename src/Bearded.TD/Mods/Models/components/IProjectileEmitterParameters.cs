using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface IProjectileEmitterParameters : ITechEffectModifiable
    {
        [Modifiable]
        ProjectileBlueprint Projectile { get; }

        [Modifiable(.15, Type = ModificationType.Cooldown)]
        TimeSpan ShootInterval { get; }

        [Modifiable(10.0)]
        Speed MuzzleVelocity { get; }
    }
}
