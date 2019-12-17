using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Technologies
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
