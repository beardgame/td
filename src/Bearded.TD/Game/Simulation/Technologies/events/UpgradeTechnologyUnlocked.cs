using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies
{
    readonly struct UpgradeTechnologyUnlocked : IGlobalEvent
    {
        public Faction Faction { get; }
        public IUpgradeBlueprint Blueprint { get; }

        public UpgradeTechnologyUnlocked(Faction faction, IUpgradeBlueprint blueprint)
        {
            Faction = faction;
            Blueprint = blueprint;
        }
    }
}
