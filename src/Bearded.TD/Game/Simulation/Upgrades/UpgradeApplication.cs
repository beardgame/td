using System.Diagnostics.CodeAnalysis;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

static partial class UpgradeApplication
{
    public static bool TryApplyUpgrade(
        this GameObject subject, IUpgrade upgrade, [NotNullWhen(true)] out IUpgradeReceipt? receipt)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);

        if (!upgradePreview.WouldBeEffective())
        {
            receipt = null;
            return false;
        }

        var operation = upgradePreview.ToOperation();
        operation.Commit();

        receipt = operation;

        return true;
    }

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

    public static IUpgradeReceipt ApplyUpgrade(this IComponent subject, IUpgrade upgrade)
    {
        var upgradePreview = new UpgradePreview(upgrade);
        subject.PreviewUpgrade(upgradePreview);
        var operation = upgradePreview.ToOperation();
        operation.Commit();
        return operation;
    }
}
