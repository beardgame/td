using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Workers
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
