using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies;

sealed class UpgradeUnlock : ITechnologyUnlock
{
    private readonly IPermanentUpgrade permanentUpgrade;

    public string Description => $"Unlock upgrade: {permanentUpgrade.Name}";

    public UpgradeUnlock(IPermanentUpgrade permanentUpgrade)
    {
        this.permanentUpgrade = permanentUpgrade;
    }

    public void Apply(FactionTechnology factionTechnology)
    {
        factionTechnology.UnlockUpgrade(permanentUpgrade);
    }
}