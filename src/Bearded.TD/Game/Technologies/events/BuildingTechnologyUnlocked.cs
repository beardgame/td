using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Technologies
{
    sealed class BuildingTechnologyUnlocked : IEvent
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
