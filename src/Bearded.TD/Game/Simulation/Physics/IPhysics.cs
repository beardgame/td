using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

interface IPhysics
{
    void ApplyVelocityImpulse(Velocity3 impulse);
    Velocity3 Velocity { get; }
}
