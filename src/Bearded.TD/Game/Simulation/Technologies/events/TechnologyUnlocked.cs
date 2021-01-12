using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct TechnologyUnlocked : IGlobalEvent
    {
        public ITechnologyBlueprint Technology { get; }

        public TechnologyUnlocked(ITechnologyBlueprint technology)
        {
            Technology = technology;
        }
    }
}
