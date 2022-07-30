using System;
using System.Collections.Generic;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    public static bool CanApplyUpgrade(this GameObject subject, IUpgrade upgrade)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);
        return upgradePreview.WouldBeEffective();
    }

    public static IUpgradeReceipt ApplyUpgrade(this GameObject subject, IUpgrade upgrade)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);
        var operation = upgradePreview.ToOperation();
        operation.Commit();
        return operation;
    }
}
