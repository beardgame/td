using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UpgradeTechnologyUnlocked : IEvent
    {
        private readonly IUpgradeBlueprint blueprint;

        public UpgradeTechnologyUnlocked(IUpgradeBlueprint blueprint)
        {
            this.blueprint = blueprint;
        }
    }
}
