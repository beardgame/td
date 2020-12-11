using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies
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
