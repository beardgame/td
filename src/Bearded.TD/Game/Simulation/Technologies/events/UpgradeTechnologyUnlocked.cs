using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly struct UpgradeTechnologyUnlocked : IGlobalEvent
{
    public FactionTechnology FactionTechnology { get; }
    public IUpgradeBlueprint Blueprint { get; }

    public UpgradeTechnologyUnlocked(FactionTechnology factionTechnology, IUpgradeBlueprint blueprint)
    {
        FactionTechnology = factionTechnology;
        Blueprint = blueprint;
    }
}