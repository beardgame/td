using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly struct UpgradeTechnologyUnlocked : IGlobalEvent
{
    public FactionTechnology FactionTechnology { get; }
    public IPermanentUpgrade Blueprint { get; }

    public UpgradeTechnologyUnlocked(FactionTechnology factionTechnology, IPermanentUpgrade blueprint)
    {
        FactionTechnology = factionTechnology;
        Blueprint = blueprint;
    }
}