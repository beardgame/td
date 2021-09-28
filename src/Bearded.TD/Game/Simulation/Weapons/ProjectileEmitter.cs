using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons
{
    [Component("projectileEmitter")]
    sealed class ProjectileEmitter : WeaponCycleHandler<IProjectileEmitterParameters>
    {
        private Instant nextPossibleShootTime;
        private bool firstShotInBurst = true;

        private Maybe<IEnemyUnitTargeter> targeter;

        public ProjectileEmitter(IProjectileEmitterParameters parameters)
            : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            targeter = ((IComponentOwner)Weapon).GetComponents<IEnemyUnitTargeter>().MaybeFirst();
        }

        public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
            => base.CanApplyUpgradeEffect(effect)
                || Parameters.Projectile.CanApplyUpgradeEffect<Projectile>(effect)
                || effect.CanApplyToComponentCollectionForType<Projectile>();

        protected override void UpdateIdle(TimeSpan elapsedTime)
        {
            firstShotInBurst = true;
        }

        protected override void UpdateShooting(TimeSpan elapsedTime)
        {
            var currentTime = Game.Time;
            while (nextPossibleShootTime < currentTime)
            {
                emitProjectile();

                if (firstShotInBurst)
                {
                    nextPossibleShootTime = currentTime + 1 / Parameters.FireRate;
                    firstShotInBurst = false;
                }
                else
                {
                    nextPossibleShootTime += 1 / Parameters.FireRate;
                }
            }
        }

        private void emitProjectile()
        {
            var velocityZ = targeter.Match(verticalSpeedCompensationToTarget, () => Speed.Zero);

            var direction = Weapon.CurrentDirection + Parameters.Spread * StaticRandom.Float(-1, 1);
            var velocityXY = direction * Parameters.MuzzleSpeed;

            Game.Add(
                new Projectile(
                    Parameters.Projectile,
                    Weapon.Position,
                    velocityXY.WithZ(velocityZ),
                    Weapon.Owner as Building
                )
            );
        }

        private Speed verticalSpeedCompensationToTarget(IEnemyUnitTargeter targeter)
        {
            var difference = targeter.Target.Position - Weapon.Position;
            var distance = difference.Length;
            // TODO: the division below should work in spacetime
            var expectedTimeToTarget = new TimeSpan(distance.NumericValue / Parameters.MuzzleSpeed.NumericValue);
            // TODO: TimeSpan.Squared would be nice to have
            var expectedDrop = Constants.Game.Physics.Gravity * expectedTimeToTarget * expectedTimeToTarget * 0.5f;
            var heightDifference = difference.Z;

            var heightToCompensate = heightDifference - expectedDrop;
            var verticalVelocityToCompensate = heightToCompensate / expectedTimeToTarget;

            return verticalVelocityToCompensate;
        }
    }
}
