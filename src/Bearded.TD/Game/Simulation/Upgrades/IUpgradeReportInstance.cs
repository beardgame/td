using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeReportInstance : IDisposable
{
    bool CanPlayerUpgradeBuilding { get; }
    ResourceAmount PlayerResources { get; }
    IReadOnlyCollection<IUpgradeModel> Upgrades { get; }
    IReadOnlyCollection<IPermanentUpgrade> AvailableUpgrades { get; }
    int OccupiedUpgradeSlots { get; }
    int UnlockedUpgradeSlots { get; }

    event VoidEventHandler? UpgradesUpdated;
    event VoidEventHandler? AvailableUpgradesUpdated;

    void QueueUpgrade(IPermanentUpgrade upgrade);

    interface IUpgradeModel
    {
        IPermanentUpgrade Blueprint { get; }
        double Progress { get; }
        bool IsFinished { get; }
    }
}
