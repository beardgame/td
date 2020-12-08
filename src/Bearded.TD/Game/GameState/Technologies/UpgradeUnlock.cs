using Bearded.TD.Game.GameState.Upgrades;

namespace Bearded.TD.Game.GameState.Technologies
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
