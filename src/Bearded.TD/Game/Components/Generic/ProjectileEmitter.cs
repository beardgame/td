using Bearded.TD.Content.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Upgrades;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Generic
{
    [Component("projectileEmitter")]
    sealed class ProjectileEmitter : WeaponCycleHandler<IProjectileEmitterParameters>
    {
        private Instant nextPossibleShootTime;
        private bool wasShootingLastFrame;

        public ProjectileEmitter(IProjectileEmitterParameters parameters)
            : base(parameters)
        {
        }

        public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
            => base.CanApplyUpgradeEffect(effect)
               || Parameters.Projectile.CanApplyUpgradeEffect(effect);

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
                    nextPossibleShootTime = currentTime + 1 / Parameters.FireRate;
                    break;
                }

                nextPossibleShootTime += 1 / Parameters.FireRate;
            }
            wasShootingLastFrame = true;
        }

        private void emitProjectile()
        {
            Game.Add(
                new Projectile(
                    Parameters.Projectile,
                    Weapon.Position, Weapon.CurrentDirection,
                    Parameters.MuzzleVelocity,
                    Weapon.Owner as Building
                )
            );
        }
    }
}
