using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

[Component("groundLockedMovement")]
sealed class GroundLockedMovement
    : Component, IPhysics, IDirected3
{
    public Velocity3 Velocity { get; private set; }
    public Difference3 Direction => Velocity * 1.S();

    protected override void OnAdded()
    {
    }

    public override void Activate()
    {
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var step = Velocity * elapsedTime;

        var movement = new PreviewMove(Owner.Position, step);
        Events.Preview(ref movement);

        var positionXY = (movement.Start + movement.Step).XY();
        var positionZ = Owner.Game.GeometryLayer[Level.GetTile(positionXY)].DrawInfo.Height;

        Owner.Position = positionXY.WithZ(positionZ);
        Owner.Direction = Direction2.Of(Velocity.NumericValue.Xy);
    }

    public void ApplyVelocityImpulse(Velocity3 impulse)
    {
        Velocity += impulse;
    }
}

