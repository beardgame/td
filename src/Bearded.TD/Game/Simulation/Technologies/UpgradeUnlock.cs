using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies;

sealed class UpgradeUnlock : ITechnologyUnlock
{
    private readonly IPermanentUpgrade upgrade;

    public string Description => $"Unlock upgrade: {upgrade.Name}";

    public UpgradeUnlock(IPermanentUpgrade upgrade)
    {
        this.upgrade = upgrade;
    }

    public void Apply(FactionTechnology factionTechnology)
    {
        factionTechnology.UnlockUpgrade(upgrade);
    }
}
