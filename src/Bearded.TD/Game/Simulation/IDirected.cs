using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation
{
    interface IDirected
    {
        Direction2 Direction { get; }
    }

    interface IDirected3
    {
        Difference3 Direction { get; }
    }
}
