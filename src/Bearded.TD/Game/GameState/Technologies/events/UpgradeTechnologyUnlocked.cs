using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.Upgrades;

namespace Bearded.TD.Game.GameState.Technologies
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
