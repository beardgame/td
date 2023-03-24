using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("drag")]
sealed class Drag : Component<Drag.IParameters>
{
    private IPhysics physics = null!;

    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(0, Type = AttributeType.Drag)]
        float Amount { get; }
    }

    public Drag(IParameters parameters) : base(parameters)
    {
    }

    protected override void OnAdded()
    {
        ComponentDependencies.Depend<IPhysics>(Owner, Events, p => physics = p);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var velocity = physics.Velocity;

        var dragForce = velocity.LengthSquared.NumericValue * Parameters.Amount;
        var direction = velocity.NumericValue.NormalizedSafe();

        var dragAcceleration = new Acceleration3(direction * -dragForce);

        physics.ApplyVelocityImpulse(dragAcceleration * elapsedTime);
    }
}

