using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct TechnologyUnlocked : IGlobalEvent
    {
        public Faction Faction { get; }
        public ITechnologyBlueprint Technology { get; }

        public TechnologyUnlocked(Faction faction, ITechnologyBlueprint technology)
        {
            Faction = faction;
            Technology = technology;
        }
    }
}
