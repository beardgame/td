using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Technologies
{
    struct BuildingTechnologyUnlocked : IEvent
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
