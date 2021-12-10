using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Workers;

readonly struct WorkerAdded : IGlobalEvent
{
    public IWorkerComponent Worker { get; }

    public WorkerAdded(IWorkerComponent worker)
    {
        Worker = worker;
    }
}