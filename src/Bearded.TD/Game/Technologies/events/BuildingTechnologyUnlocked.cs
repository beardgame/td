using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Technologies
{
    sealed class BuildingTechnologyUnlocked : IEvent
    {
        public IBuildingBlueprint Blueprint { get; }

        public BuildingTechnologyUnlocked(IBuildingBlueprint blueprint)
        {
            Blueprint = blueprint;
        }
    }
}
