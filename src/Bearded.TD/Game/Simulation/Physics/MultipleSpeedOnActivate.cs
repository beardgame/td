using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("multipleSpeedOnActivate")]
sealed class MultipleSpeedOnActivate : Component<MultipleSpeedOnActivate.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        float Factor { get; }
    }

    public MultipleSpeedOnActivate(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
        if (Owner.GetComponents<IPhysics>().FirstOrDefault() is not { } physics)
            return;

        var velocity = physics.Velocity;
        var velocityImpulse = velocity * Parameters.Factor;
        physics.ApplyVelocityImpulse(velocityImpulse - velocity);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }
}

