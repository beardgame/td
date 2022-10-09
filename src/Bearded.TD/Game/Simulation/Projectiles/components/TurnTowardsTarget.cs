using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("turnTowardsTarget")]
sealed class TurnTowardsTarget : Component<TurnTowardsTarget.IParameters>
{
    private IPositionable? target;
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        AngularVelocity TurnSpeed { get; }
    }

    public TurnTowardsTarget(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IProperty<TargetPosition>>(Owner, Events, p => target = p.Value.Target);
        ComponentDependencies.Depend<IProperty<Target>>(Owner, Events, t => target ??= t.Value.Object);
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (target == null)
            return;

        var currentVelocity = physics.Velocity;

        var rotation = getRotationTowards(target.Position, elapsedTime);

        var newVelocity = applyRotation(currentVelocity, rotation);

        var velocityImpulse = newVelocity - currentVelocity;

        physics.ApplyVelocityImpulse(velocityImpulse);
    }

    private Quaternion getRotationTowards(Position3 targetPosition, TimeSpan elapsedTime)
    {
        var direction = physics.Velocity.NumericValue.NormalizedSafe();
        var towardsTarget = (targetPosition - Owner.Position).NumericValue.NormalizedSafe();

        var rotationAxis = Vector3.Cross(direction, towardsTarget);
        var rotationAngle = Parameters.TurnSpeed * elapsedTime;

        return Quaternion.FromAxisAngle(rotationAxis, rotationAngle.Radians);
    }

    private static Velocity3 applyRotation(Velocity3 currentVelocity, Quaternion rotation)
        => new (Vector3.Transform(currentVelocity.NumericValue, rotation));
}

