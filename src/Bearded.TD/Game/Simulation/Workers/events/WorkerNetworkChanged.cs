using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

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
