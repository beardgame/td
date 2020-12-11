using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
{
    struct TechnologyQueued : IGlobalEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyQueued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
