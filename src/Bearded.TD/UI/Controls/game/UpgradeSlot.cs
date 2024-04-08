using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.UI.Controls;

sealed record UpgradeSlot(int Index, IPermanentUpgrade? Upgrade)
{
    public static UpgradeSlot Empty(int index) => new(index, null);
    public static UpgradeSlot FromState(IUpgradeSlot slot, int index) => new(index, slot.Upgrade);
}
