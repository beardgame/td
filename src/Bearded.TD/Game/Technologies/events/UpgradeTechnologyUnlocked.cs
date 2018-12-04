using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UpgradeTechnologyUnlocked : IEvent
    {
        private readonly UpgradeBlueprint blueprint;

        public UpgradeTechnologyUnlocked(UpgradeBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }
    }
}
