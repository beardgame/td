using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("projectileEmitter")]
sealed class ProjectileEmitter : WeaponCycleHandler<ProjectileEmitter.IParameters>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IComponentOwnerBlueprint Projectile { get; }

        [Modifiable(Type = AttributeType.Damage)]
        UntypedDamagePerSecond DamagePerSecond { get; }

        [Modifiable(1, Type = AttributeType.FireRate)]
        Frequency FireRate { get; }

        [Modifiable(10.0)]
        Speed MuzzleSpeed { get; }

        [Modifiable(0.0, Type = AttributeType.SpreadAngle)]
        Angle Spread { get; }

        Unit MuzzleOffset { get; }
    }

    private Instant nextPossibleShootTime;
    private bool firstShotInBurst = true;

    private ITargeter<IPositionable>? targeter;

    public ProjectileEmitter(IParameters parameters)
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
            || Parameters.Projectile.CanApplyUpgradeEffect(effect)
            || effect.CanApplyToComponentCollectionForType();

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

        var position = Weapon.Position + (Weapon.Direction * Parameters.MuzzleOffset).WithZ();

        ProjectileFactory.Create(
            Game, Parameters.Projectile, Owner, position, direction, muzzleVelocity, UntypedDamage.Zero);

        Events.Send(new ShotProjectile(position, direction, muzzleVelocity));
    }

    private (Direction2, Velocity3) getMuzzleVelocity()
    {
        var direction = Weapon.Direction + Parameters.Spread * StaticRandom.Float(-1, 1);
        var velocityXY = direction * Parameters.MuzzleSpeed;

        var velocityZ = targeter?.Target is { } target
            ? verticalSpeedCompensationToTarget(target)
            : Speed.Zero;

        return (direction, velocityXY.WithZ(velocityZ));
    }

    private Speed verticalSpeedCompensationToTarget(IPositionable target)
    {
        var difference = target.Position - Weapon.Position;
        var horizontalDistance = difference.XY().Length;
        // TODO: the division below should work in spacetime
        var expectedTimeToTarget = new TimeSpan(horizontalDistance.NumericValue / Parameters.MuzzleSpeed.NumericValue);
        // TODO: TimeSpan.Squared would be nice to have
        var expectedDrop = Constants.Game.Physics.Gravity * expectedTimeToTarget * expectedTimeToTarget * 0.5f;
        var heightDifference = difference.Z;

        var heightToCompensate = heightDifference - expectedDrop;
        var verticalVelocityToCompensate = heightToCompensate / expectedTimeToTarget;

        return verticalVelocityToCompensate;
    }
}
