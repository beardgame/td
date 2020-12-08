using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Technologies
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
