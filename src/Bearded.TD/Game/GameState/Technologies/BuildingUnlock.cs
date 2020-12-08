using Bearded.TD.Game.GameState.Buildings;

namespace Bearded.TD.Game.GameState.Technologies
{
    sealed class BuildingUnlock : ITechnologyUnlock
    {
        private readonly IBuildingBlueprint buildingBlueprint;

        public string Description => $"Unlock building: {buildingBlueprint.Name}";

        public BuildingUnlock(IBuildingBlueprint buildingBlueprint)
        {
            this.buildingBlueprint = buildingBlueprint;
        }

        public void Apply(TechnologyManager technologyManager)
        {
            technologyManager.UnlockBuilding(buildingBlueprint);
        }
    }
}
