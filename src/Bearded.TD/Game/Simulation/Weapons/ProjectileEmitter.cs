using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("projectileEmitter")]
sealed class ProjectileEmitter : Component<ProjectileEmitter.IParameters>, IListener<ShootProjectile>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IComponentOwnerBlueprint Projectile { get; }

        [Modifiable(10.0)]
        Speed MuzzleSpeed { get; }

        [Modifiable(0.0, Type = AttributeType.SpreadAngle)]
        Angle Spread { get; }

        Difference2 MuzzleOffset { get; }
    }

    private IWeaponState weapon = null!;
    private ITargeter<IPositionable>? targeter;

    public ProjectileEmitter(IParameters parameters)
        : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        ComponentDependencies.DependDynamic<ITargeter<IPositionable>>(Owner, Events, c => targeter = c);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public override bool CanApplyUpgradeEffect(IUpgradeEffect effect)
        => base.CanApplyUpgradeEffect(effect) || Parameters.Projectile.CanApplyUpgradeEffect(effect);

    public void HandleEvent(ShootProjectile @event)
    {
        emitProjectile(@event.Damage);
    }

    private void emitProjectile(UntypedDamage damage)
    {
        var (direction, muzzleVelocity) = getMuzzleVelocity();

        var weaponDirection = weapon.Direction.Vector;
        var position = weapon.Position +
            (weaponDirection * Parameters.MuzzleOffset.X
                + weaponDirection.PerpendicularLeft * Parameters.MuzzleOffset.Y
            ).WithZ();

        ProjectileFactory.Create(
            Owner.Game,
            Parameters.Projectile,
            Owner,
            position,
            direction,
            muzzleVelocity,
            damage);

        Events.Send(new ShotProjectile(position, direction, muzzleVelocity));
    }

    private (Direction2, Velocity3) getMuzzleVelocity()
    {
        var direction = weapon.Direction + Parameters.Spread * StaticRandom.Float(-1, 1);
        var velocityXY = direction * Parameters.MuzzleSpeed;

        var velocityZ = targeter?.Target is { } target
            ? verticalSpeedCompensationToTarget(target)
            : Speed.Zero;

        return (direction, velocityXY.WithZ(velocityZ));
    }

    private Speed verticalSpeedCompensationToTarget(IPositionable target)
    {
        var difference = target.Position - weapon.Position;
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
