using System;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Weapons;

[Component("linearAccelerationAim")]
class LinearAccelerationAim : Component<Weapon, ILinearAccelerationAimParameters>, IAngularAccelerator
{
    private AngularVelocity angularVelocity;
    private IWeaponAimer? aimer;

    public LinearAccelerationAim(ILinearAccelerationAimParameters parameters) : base(parameters) { }

    public void Impact(AngularVelocity impact)
    {
        angularVelocity += impact;
    }

    protected override void OnAdded()
    {
        ComponentDependencies.DependDynamic<IWeaponAimer>(Owner, Events, c => aimer = c);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.IsEnabled)
        {
            accelerateTowardsGoal(elapsedTime);
            applyVelocity(elapsedTime);
        }

        dampVelocity(elapsedTime);
    }

    private void accelerateTowardsGoal(TimeSpan elapsedTime)
    {
        if (aimer != null)
            aimIn(aimer.AimDirection, elapsedTime);
    }

    private void aimIn(Direction2 aimDirection, TimeSpan elapsedTime)
    {
        var angularDirection = Angle.Between(Owner.CurrentDirection, aimDirection).Sign();
        var acceleration = Parameters.Acceleration * angularDirection;

        angularVelocity += acceleration * elapsedTime;
    }

    private void applyVelocity(TimeSpan elapsedTime)
    {
        Owner.Turn(angularVelocity * elapsedTime);
    }

    private void dampVelocity(TimeSpan elapsedTime)
    {
        var damping = 1 - 1 / (Parameters.DragInverse + 1);
        var velocityMultiplier = (float)Math.Pow(damping, elapsedTime.NumericValue);

        angularVelocity *= velocityMultiplier;
    }
}
