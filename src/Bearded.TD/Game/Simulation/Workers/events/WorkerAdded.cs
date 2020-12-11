using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Workers
{
    readonly struct WorkerAdded : IGlobalEvent
    {
        public Worker Worker { get; }

        public WorkerAdded(Worker worker)
        {
            Worker = worker;
        }
    }
}
