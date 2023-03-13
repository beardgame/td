using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("accelerateForwards")]
sealed class AccelerateForwards : Component<AccelerateForwards.IParameters>
{
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        Acceleration Acceleration { get; }
    }

    public AccelerateForwards(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var direction = physics.Velocity.NumericValue.NormalizedSafe();

        var acceleration = Parameters.Acceleration * direction;
        var velocityImpulse = acceleration * elapsedTime;

        physics.ApplyVelocityImpulse(velocityImpulse);
    }
}

