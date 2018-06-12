using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Mods.Models;
using Bearded.Utilities;

namespace Bearded.TD.Game.Technologies
{
    sealed class TechnologyManager
    {
        private readonly HashSet<BuildingBlueprint> unlockedBuildings = new HashSet<BuildingBlueprint>();

        public IEnumerable<BuildingBlueprint> UnlockedBuildings => unlockedBuildings.ToList();

        public event GenericEventHandler<BuildingBlueprint> BuildingUnlocked;

        public void UnlockBlueprint(BuildingBlueprint blueprint)
        {
            // This should probably be "unlock technology" or something like that, but we don't have the concept of
            // technology yet.

            unlockedBuildings.Add(blueprint);
            BuildingUnlocked?.Invoke(blueprint);
        }
    }
}
