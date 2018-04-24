using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    sealed class ProjectileEmitterParameters
    {
        public ProjectileBlueprint Projectile { get; }
        public TimeSpan ShootInterval { get; } = new TimeSpan(0.15);
        public Speed MuzzleVelocity { get; } = 10.U() / 1.S();

        public ProjectileEmitterParameters(
            ProjectileBlueprint projectile,
            TimeSpan? shootInterval, Speed? muzzleVelocity)
        {
            Projectile = projectile;

            ShootInterval = shootInterval ?? ShootInterval;
            MuzzleVelocity = muzzleVelocity ?? MuzzleVelocity;
        }
    }
}