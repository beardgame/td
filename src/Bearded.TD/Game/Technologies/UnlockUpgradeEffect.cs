using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UnlockUpgradeEffect : ITechnologyEffect
    {
        private readonly UpgradeBlueprint upgradeBlueprint;

        public string Description => $"Unlock upgrade: {upgradeBlueprint.Name}";

        public UnlockUpgradeEffect(UpgradeBlueprint upgradeBlueprint)
        {
            this.upgradeBlueprint = upgradeBlueprint;
        }

        public void Unlock(TechnologyManager technologyManager)
        {
            technologyManager.UnlockUpgrade(upgradeBlueprint);
        }
    }
}
