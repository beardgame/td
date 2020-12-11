using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Technologies
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
