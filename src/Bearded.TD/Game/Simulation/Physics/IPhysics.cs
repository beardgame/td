using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

interface IPhysics : IMoving
{
    void ApplyVelocityImpulse(Velocity3 impulse);
}
