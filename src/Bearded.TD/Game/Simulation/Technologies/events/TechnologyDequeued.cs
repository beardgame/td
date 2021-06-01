using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct TechnologyDequeued : IGlobalEvent
    {
        public FactionTechnology FactionTechnology { get; }
        public ITechnologyBlueprint Technology { get; }

        public TechnologyDequeued(FactionTechnology factionTechnology, ITechnologyBlueprint technology)
        {
            FactionTechnology = factionTechnology;
            Technology = technology;
        }
    }
}
