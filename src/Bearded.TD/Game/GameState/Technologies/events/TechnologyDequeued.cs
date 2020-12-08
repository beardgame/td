using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Technologies
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
