using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Technologies
{
    struct BuildingTechnologyUnlocked : IGlobalEvent
    {
        public TechnologyManager TechnologyManager { get; }
        public IBuildingBlueprint Blueprint { get; }

        public BuildingTechnologyUnlocked(TechnologyManager technologyManager, IBuildingBlueprint blueprint)
        {
            TechnologyManager = technologyManager;
            Blueprint = blueprint;
        }
    }
}
