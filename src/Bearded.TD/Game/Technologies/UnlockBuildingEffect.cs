using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Technologies
{
    sealed class UnlockBuildingEffect : ITechnologyEffect
    {
        private readonly IBuildingBlueprint buildingBlueprint;

        public string Description => $"Unlock building: {buildingBlueprint.Name}";

        public UnlockBuildingEffect(IBuildingBlueprint buildingBlueprint)
        {
            this.buildingBlueprint = buildingBlueprint;
        }

        public void Unlock(TechnologyManager technologyManager)
        {
            technologyManager.UnlockBuilding(buildingBlueprint);
        }
    }
}
