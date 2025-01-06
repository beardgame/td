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
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

interface IProjectileEmitter
{
    Position3 EmitPosition { get; }
    Speed MuzzleSpeed { get; }
}

[Component("projectileEmitter")]
sealed class ProjectileEmitter(ProjectileEmitter.IParameters parameters)
    : Component<ProjectileEmitter.IParameters>(parameters), IListener<FireWeapon>, IProjectileEmitter
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        IGameObjectBlueprint Projectile { get; }

        [Modifiable(10.0, Type = AttributeType.MuzzleSpeed)]
        Speed MuzzleSpeed { get; }

        [Modifiable(1, Type = AttributeType.BulletDropCompensation)]
        float BulletDropCompensation { get; }

        [Modifiable(0.0, Type = AttributeType.SpreadAngle)]
        Angle Spread { get; }

        Difference2 MuzzleOffset { get; }

        [Modifiable(1, Type = AttributeType.ShotIntervalCount)]
        int ShootEvery { get; }
        int ShootEveryOffset { get; }
    }

    private IWeaponState weapon = null!;
    private ITargeter<IPositionable>? targeter;
    private UpgradableProjectileFactory factory = null!;
    private int shotCounter;

    public Position3 EmitPosition
    {
        get
        {
            var weaponDirection = weapon.Direction.Vector;
            return weapon.Position +
                (weaponDirection * Parameters.MuzzleOffset.X
                    + weaponDirection.PerpendicularLeft * Parameters.MuzzleOffset.Y
                ).WithZ();
        }
    }

    public Speed MuzzleSpeed => Parameters.MuzzleSpeed;

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        ComponentDependencies.DependDynamic<ITargeter<IPositionable>>(Owner, Events, c => targeter = c);
        factory = new UpgradableProjectileFactory(Parameters.Projectile, Owner);
        Events.Subscribe(this);

        shotCounter = Parameters.ShootEveryOffset;
    }

    public override void Update(TimeSpan elapsedTime) {}

    public override void PreviewUpgrade(IUpgradePreview upgradePreview)
    {
        base.PreviewUpgrade(upgradePreview);
        factory.PreviewUpgrade(upgradePreview);
    }

    public void HandleEvent(FireWeapon @event)
    {
        shotCounter++;
        if (Parameters.ShootEvery <= 1 || shotCounter % Parameters.ShootEvery == 0)
            emitProjectile(@event.Damage);
    }

    private void emitProjectile(UntypedDamage damage)
    {
        var position = EmitPosition;

        var (direction, muzzleVelocity) = getMuzzleVelocity(position);

        var projectile = factory.Create(position, direction, muzzleVelocity, damage,
            new OptionalProjectileProperties
            {
                Target = targeter?.Target as GameObject,
                TargetPosition = targeter?.Target,
                Source = Owner,
            });

        Owner.Game.Add(projectile);

        Events.Send(new ShotProjectile(position, direction, muzzleVelocity, projectile, damage));
    }

    private (Direction2, Velocity3) getMuzzleVelocity(Position3 emitLocation)
    {
        var direction = weapon.Direction + Parameters.Spread * StaticRandom.Float(-1, 1);
        var velocityXY = direction * Parameters.MuzzleSpeed;

        var velocityZ = targeter?.Target is { } target
            ? verticalSpeedCompensationToTarget(emitLocation, target)
            : Speed.Zero;

        return (direction, velocityXY.WithZ(velocityZ));
    }

    private Speed verticalSpeedCompensationToTarget(Position3 emitLocation, IPositionable target)
    {
        if (Parameters.BulletDropCompensation == 0)
            return Speed.Zero;

        var difference = target.Position - emitLocation;
        var horizontalDistance = difference.XY().Length;
        // TODO: the division below should work in spacetime
        var expectedTimeToTarget = new TimeSpan(horizontalDistance.NumericValue / Parameters.MuzzleSpeed.NumericValue);
        // TODO: TimeSpan.Squared would be nice to have
        var expectedDrop = Constants.Game.Physics.Gravity * expectedTimeToTarget * expectedTimeToTarget * 0.5f;
        var heightDifference = difference.Z;

        var heightToCompensate = heightDifference - expectedDrop;
        var verticalVelocityToCompensate = heightToCompensate / expectedTimeToTarget;

        return verticalVelocityToCompensate * Parameters.BulletDropCompensation;
    }
}
