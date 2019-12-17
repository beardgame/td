using Bearded.TD.Game.Events;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    struct UpgradeTechnologyUnlocked : IGlobalEvent
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
