using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct TechnologyQueued : IGlobalEvent
    {
        public FactionTechnology FactionTechnology { get; }
        public ITechnologyBlueprint Technology { get; }

        public TechnologyQueued(FactionTechnology factionTechnology, ITechnologyBlueprint technology)
        {
            FactionTechnology = factionTechnology;
            Technology = technology;
        }
    }
}
