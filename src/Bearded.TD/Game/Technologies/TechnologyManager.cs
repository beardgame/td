using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Upgrades;
using Bearded.Utilities;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager
    {
        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new HashSet<IBuildingBlueprint>();
        private readonly List<UpgradeBlueprint> unlockedUpgrades = new List<UpgradeBlueprint>();

        public IEnumerable<IBuildingBlueprint> UnlockedBuildings => unlockedBuildings.ToList();

        public event GenericEventHandler<IBuildingBlueprint> BuildingUnlocked;

        public void UnlockBlueprint(IBuildingBlueprint blueprint)
        {
            // This should probably be "unlock technology" or something like that, but we don't have the concept of
            // technology yet.

            unlockedBuildings.Add(blueprint);
            BuildingUnlocked?.Invoke(blueprint);
        }

        public IEnumerable<UpgradeBlueprint> GetApplicableUpgradesFor(Building building)
        {
            var components = new ComponentCollection<Building>();
            return unlockedUpgrades.Where(upgrade => upgrade.CanApplyTo(components));
        }
    }
}
