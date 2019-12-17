using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Technologies
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
