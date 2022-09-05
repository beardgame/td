using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics;

sealed class ParabolicMovement : Component, IDirected3
{
    private Velocity3 velocity;

    public Difference3 Direction => velocity * 1.S();

    public ParabolicMovement(Velocity3 velocity)
    {
        this.velocity = velocity;
    }

    protected override void OnAdded()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var forces = Constants.Game.Physics.Gravity3;

        velocity += forces * elapsedTime;
        var step = velocity * elapsedTime;

        var movement = new PreviewMove(Owner.Position, step);
        Events.Preview(ref movement);

        Owner.Position = movement.Start + movement.Step;
    }
}
