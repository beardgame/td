using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager
    {
        private readonly GameEvents events;

        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new HashSet<IBuildingBlueprint>();
        private readonly List<UpgradeBlueprint> unlockedUpgrades = new List<UpgradeBlueprint>();

        public TechnologyManager(GameEvents events)
        {
            this.events = events;
        }
        
        // The following 2 methods should probably be "unlock technology" or something like that, but we don't have the
        // concept of technology yet.

        public void UnlockBlueprint(IBuildingBlueprint blueprint)
        {
            unlockedBuildings.Add(blueprint);
            events.Send(new BuildingTechnologyUnlocked(blueprint));
        }

        public void UnlockUpgrade(UpgradeBlueprint blueprint)
        {
            unlockedUpgrades.Add(blueprint);
            events.Send(new UpgradeTechnologyUnlocked(blueprint));
        }

        public IEnumerable<UpgradeBlueprint> GetApplicableUpgradesFor(Building building)
        {
            return unlockedUpgrades.Where(building.CanApplyUpgrade);
        }
    }
}
