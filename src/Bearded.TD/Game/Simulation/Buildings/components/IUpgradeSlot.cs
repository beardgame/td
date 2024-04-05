using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings;

interface IUpgradeSlot
{
    IPermanentUpgrade? Upgrade { get; }

    bool Filled => Upgrade is not null;
}
