using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
{
    struct TechnologyDequeued : IGlobalEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyDequeued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
