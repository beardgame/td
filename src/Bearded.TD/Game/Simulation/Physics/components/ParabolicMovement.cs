using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class ParabolicMovement : Component, IDirected3, IPhysics
{
    public Velocity3 Velocity { get; private set; }

    public Difference3 Direction => Velocity * 1.S();

    public ParabolicMovement(Velocity3 velocity)
    {
        Velocity = velocity;
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var forces = Constants.Game.Physics.Gravity3;

        Velocity += forces * elapsedTime;
        var step = Velocity * elapsedTime;

        var movement = new PreviewMove(Owner.Position, step);
        Events.Preview(ref movement);

        Owner.Position = movement.Start + movement.Step;
        Owner.Direction = Direction2.Of(Velocity.NumericValue.Xy);
    }

    public void ApplyVelocityImpulse(Velocity3 impulse)
    {
        Velocity += impulse;
    }
}
