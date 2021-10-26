using Bearded.TD.Game.Simulation.Buildings;

namespace Bearded.TD.Game.Simulation.Technologies
{
    sealed class BuildingUnlock : ITechnologyUnlock
    {
        private readonly IBuildingBlueprint buildingBlueprint;

        public string Description => $"Unlock building: {buildingBlueprint.GetName()}";

        public BuildingUnlock(IBuildingBlueprint buildingBlueprint)
        {
            this.buildingBlueprint = buildingBlueprint;
        }

        public void Apply(FactionTechnology factionTechnology)
        {
            factionTechnology.UnlockBuilding(buildingBlueprint);
        }
    }
}
