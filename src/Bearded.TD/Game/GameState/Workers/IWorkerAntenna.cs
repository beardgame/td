using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.GameState.Workers
{
    public interface IWorkerAntenna
    {
        Position2 Position { get; }
        Unit WorkerRange { get; }
    }
}
