using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Buildings.Ruins
{
    readonly struct RepairFinished : IComponentEvent
    {
        public Faction RepairingFaction { get; }

        public RepairFinished(Faction repairingFaction)
        {
            RepairingFaction = repairingFaction;
        }
    }
}
