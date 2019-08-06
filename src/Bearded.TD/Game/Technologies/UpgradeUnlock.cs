using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UpgradeUnlock : ITechnologyUnlock
    {
        private readonly IUpgradeBlueprint upgradeBlueprint;

        public string Description => $"Unlock upgrade: {upgradeBlueprint.Name}";

        public UpgradeUnlock(IUpgradeBlueprint upgradeBlueprint)
        {
            this.upgradeBlueprint = upgradeBlueprint;
        }

        public void Apply(TechnologyManager technologyManager)
        {
            technologyManager.UnlockUpgrade(upgradeBlueprint);
        }
    }
}
