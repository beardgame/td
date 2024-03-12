using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed record UpgradeSlot(IPermanentUpgrade? Upgrade, UpgradeSlot.IUnlockProgress? UnlockProgress)
{
    public interface IUnlockProgress { double Progress { get; } }
}
