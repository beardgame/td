using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct TechnologyQueued : IGlobalEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyQueued(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
