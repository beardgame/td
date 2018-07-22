using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.Utilities;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager
    {
        private readonly HashSet<IBuildingBlueprint> unlockedBuildings = new HashSet<IBuildingBlueprint>();

        public IEnumerable<IBuildingBlueprint> UnlockedBuildings => unlockedBuildings.ToList();

        public event GenericEventHandler<IBuildingBlueprint> BuildingUnlocked;

        public void UnlockBlueprint(IBuildingBlueprint blueprint)
        {
            // This should probably be "unlock technology" or something like that, but we don't have the concept of
            // technology yet.

            unlockedBuildings.Add(blueprint);
            BuildingUnlocked?.Invoke(blueprint);
        }
    }
}
