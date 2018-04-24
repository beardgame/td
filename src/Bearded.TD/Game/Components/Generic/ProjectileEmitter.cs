using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("projectileEmitter")]
    sealed class ProjectileEmitter : WeaponCycleHandler<ProjectileEmitterParameters>
    {
        private Instant nextPossibleShootTime;
        private bool wasShootingLastFrame;

        public ProjectileEmitter(ProjectileEmitterParameters parameters)
            : base(parameters)
        {
        }

        protected override void UpdateIdle(TimeSpan elapsedTime)
        {
            wasShootingLastFrame = false;
        }

        protected override void UpdateShooting(TimeSpan elapsedTime)
        {
            var currentTime = Game.Time;
            while (nextPossibleShootTime < currentTime)
            {
                emitProjectile();

                if (!wasShootingLastFrame)
                {
                    nextPossibleShootTime = currentTime + Parameters.ShootInterval;
                    break;
                }

                nextPossibleShootTime += Parameters.ShootInterval;
            }
            wasShootingLastFrame = true;
        }

        private void emitProjectile()
        {
            Game.Add(
                new Projectile(
                    Parameters.Projectile,
                    Weapon.Position, Weapon.AimDirection,
                    Parameters.MuzzleVelocity,
                    Weapon.Owner as Building
                )
            );
        }
    }
}
