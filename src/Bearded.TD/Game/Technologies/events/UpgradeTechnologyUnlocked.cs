using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class UpgradeTechnologyUnlocked : IEvent
    {
        public TechnologyManager TechnologyManager { get; }
        public IUpgradeBlueprint Blueprint { get; }

        public UpgradeTechnologyUnlocked(TechnologyManager technologyManager, IUpgradeBlueprint blueprint)
        {
            TechnologyManager = technologyManager;
            Blueprint = blueprint;
        }
    }
}
