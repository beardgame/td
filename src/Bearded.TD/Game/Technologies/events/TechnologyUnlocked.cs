using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Technologies
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
