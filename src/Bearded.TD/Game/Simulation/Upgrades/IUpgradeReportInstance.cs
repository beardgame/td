using System;
using System.Collections.Generic;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    interface IUpgradeReportInstance : IDisposable
    {
        bool CanPlayerUpgradeBuilding { get; }
        IReadOnlyCollection<IUpgradeModel> Upgrades { get; }

        event VoidEventHandler? UpgradesUpdated;

        void QueueUpgrade(IUpgradeBlueprint upgrade);

        interface IUpgradeModel
        {
            IUpgradeBlueprint Blueprint { get; }
            double Progress { get; }
            bool IsFinished { get; }
        }
    }
}
