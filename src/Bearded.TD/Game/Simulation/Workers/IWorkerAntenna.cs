using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers
{
    interface IWorkerAntenna
    {
        Position2 Position { get; }
        Unit WorkerRange { get; }

        IArea Coverage => Area.Circular(Position, WorkerRange);
    }
}
