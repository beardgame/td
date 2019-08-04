using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UnlockUpgradeEffect : ITechnologyEffect
    {
        private readonly IUpgradeBlueprint upgradeBlueprint;

        public string Description => $"Unlock upgrade: {upgradeBlueprint.Name}";

        public UnlockUpgradeEffect(IUpgradeBlueprint upgradeBlueprint)
        {
            this.upgradeBlueprint = upgradeBlueprint;
        }

        public void Unlock(TechnologyManager technologyManager)
        {
            technologyManager.UnlockUpgrade(upgradeBlueprint);
        }
    }
}
