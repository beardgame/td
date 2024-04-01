using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.UI.Controls;

sealed record UpgradeSlot(IPermanentUpgrade? Upgrade)
{
    public static UpgradeSlot Empty() => new((IPermanentUpgrade?) null);
    public static UpgradeSlot FromState(IUpgradeSlot slot) => new(slot.Upgrade);
}
