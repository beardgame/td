using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Workers
{
    struct WorkerNetworkChanged : IGlobalEvent
    {
        public Faction Faction { get; }

        public WorkerNetworkChanged(Faction faction)
        {
            Faction = faction;
        }
    }
}
