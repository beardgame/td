using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons
{
    [Component("projectileEmitter")]
    sealed class ProjectileEmitter : WeaponCycleHandler<IProjectileEmitterParameters>
    {
        private Instant nextPossibleShootTime;
        private bool firstShotInBurst = true;

        private ITargeter<IPositionable>? targeter;

        public ProjectileEmitter(IProjectileEmitterParameters parameters)
            : base(parameters)
        {
        }

        protected override void OnAdded()
        {
            base.OnAdded();

            ComponentDependencies.DependDynamic<ITargeter<IPositionable>>(Owner, Events, c => targeter = c);
        }

        public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
            => base.CanApplyUpgradeEffect(effect)
                || Parameters.Projectile.CanApplyUpgradeEffect<ComponentGameObject>(effect)
                || effect.CanApplyToComponentCollectionForType<ComponentGameObject>();

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
            var (direction, muzzleVelocity) = getMuzzleVelocity();

            var position = Weapon.Position + (Weapon.CurrentDirection * Parameters.MuzzleOffset).WithZ();

            var projectile = ComponentGameObjectFactory.CreateFromBlueprintWithDefaultRenderer(
                Game, Parameters.Projectile, Owner, position, direction);

            projectile.AddComponent(new ParabolicMovement(muzzleVelocity));

            applyCurrentUpgradesTo(projectile);

            Events.Send(new ShotProjectile(position, direction, muzzleVelocity));
        }

        private void applyCurrentUpgradesTo(ComponentGameObject projectile)
        {
            var upgrades = (Weapon.Owner as ComponentGameObject)
                ?.GetComponents<IBuildingUpgradeManager>().SingleOrDefault()
                ?.AppliedUpgrades
                .Where(u => u.CanApplyTo(projectile))
                ?? Enumerable.Empty<IUpgradeBlueprint>();

            foreach (var upgrade in upgrades)
            {
                upgrade.ApplyTo(projectile);
            }
        }

        private (Direction2, Velocity3) getMuzzleVelocity()
        {
            var direction = Weapon.CurrentDirection + Parameters.Spread * StaticRandom.Float(-1, 1);
            var velocityXY = direction * Parameters.MuzzleSpeed;

            var velocityZ = targeter?.Target is { } target
                ? verticalSpeedCompensationToTarget(target)
                : Speed.Zero;

            return (direction, velocityXY.WithZ(velocityZ));
        }

        private Speed verticalSpeedCompensationToTarget(IPositionable target)
        {
            var difference = target.Position - Weapon.Position;
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
