using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Workers
{
    readonly struct WorkerNetworkChanged : IGlobalEvent
    {
        public WorkerNetwork Network { get; }

        public WorkerNetworkChanged(WorkerNetwork network)
        {
            Network = network;
        }
    }
}
