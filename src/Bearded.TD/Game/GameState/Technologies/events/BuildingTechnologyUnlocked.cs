using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Technologies
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
