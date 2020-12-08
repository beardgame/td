using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Technologies
{
    struct TechnologyUnlocked : IGlobalEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyUnlocked(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
