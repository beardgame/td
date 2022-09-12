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

        var velocity = physics.Velocity;
        var direction = velocity.NumericValue.NormalizedSafe();
        var towardsTarget = (target.Position - Owner.Position).NumericValue.NormalizedSafe();

        var rotationAxis = Vector3.Cross(direction, towardsTarget);
        var rotationAngle = Parameters.TurnSpeed * elapsedTime;

        var rotation = Quaternion.FromAxisAngle(rotationAxis, rotationAngle.Radians);

        var newVelocity = new Velocity3(Vector3.Transform(velocity.NumericValue, rotation));

        var impulseVelocity = newVelocity - velocity;

        physics.ApplyVelocityImpulse(impulseVelocity);
    }
}

