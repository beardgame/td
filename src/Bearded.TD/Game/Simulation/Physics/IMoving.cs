using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Physics;

interface IMoving
{
    Velocity3 Velocity { get; }
}
