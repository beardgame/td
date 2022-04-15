using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("pidAim")]
sealed class PidAim : Component<PidAim.IParameters>, IAngularAccelerator
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(100, Type = AttributeType.TurnSpeed)]
        float ProportionalCorrection { get; }

        [Modifiable(10, Type = AttributeType.TurnSpeed)]
        float DerivativeCorrection { get; }
    }

    private IWeaponState weapon = null!;
    private AngularVelocity angularVelocity;
    private IWeaponAimer? aimer;

    public PidAim(IParameters parameters) : base(parameters)
    {
    }

    public void Impact(AngularVelocity impact)
    {
        angularVelocity += impact;
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IWeaponState>(Owner, Events, c => weapon = c);
        ComponentDependencies.DependDynamic<IWeaponAimer>(Owner, Events, c => aimer = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (weapon.IsEnabled)
        {
            accelerateTowardsGoal(elapsedTime);
            applyVelocity(elapsedTime);
        }
    }

    private void accelerateTowardsGoal(TimeSpan elapsedTime)
    {
        if (aimer == null)
            return;

        var targetDirection = clampDirectionToWeaponTurningArc(aimer.AimDirection);
        aimIn(targetDirection, elapsedTime);
    }

    private Direction2 clampDirectionToWeaponTurningArc(Direction2 targetDirection)
    {
        if (weapon.MaximumTurningAngle is not { } maxAngle)
            return targetDirection;

        var neutral = weapon.NeutralDirection;

        var neutralToTarget = Angle.Between(neutral, targetDirection);

        if (neutralToTarget < -maxAngle)
            targetDirection = neutral - maxAngle;
        else if (neutralToTarget > maxAngle)
            targetDirection = neutral + maxAngle;

        return targetDirection;
    }

    private void aimIn(Direction2 aimDirection, TimeSpan elapsedTime)
    {
        var error = Angle.Between(weapon.Direction, aimDirection);
        var derivative = angularVelocity;
        var torque = new AngularAcceleration(
            Parameters.ProportionalCorrection * error
            - Parameters.DerivativeCorrection * derivative * 1.S()
            );

        angularVelocity += torque * elapsedTime;
    }

    private void applyVelocity(TimeSpan elapsedTime)
    {
        weapon.Turn(angularVelocity * elapsedTime);
    }
}
