using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies
{
    sealed class UpgradeUnlock : ITechnologyUnlock
    {
        private readonly IUpgradeBlueprint upgradeBlueprint;

        public string Description => $"Unlock upgrade: {upgradeBlueprint.Name}";

        public UpgradeUnlock(IUpgradeBlueprint upgradeBlueprint)
        {
            this.upgradeBlueprint = upgradeBlueprint;
        }

        public void Apply(FactionTechnology factionTechnology)
        {
            factionTechnology.UnlockUpgrade(upgradeBlueprint);
        }
    }
}
