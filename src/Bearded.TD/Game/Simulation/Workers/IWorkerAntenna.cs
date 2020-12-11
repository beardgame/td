using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Workers
{
    public interface IWorkerAntenna
    {
        Position2 Position { get; }
        Unit WorkerRange { get; }
    }
}
