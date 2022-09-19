using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Technologies;

sealed class BuildingUnlock : ITechnologyUnlock
{
    private readonly IGameObjectBlueprint buildingBlueprint;

    public string Description => $"Unlock building: {buildingBlueprint.GetName()}";

    public BuildingUnlock(IGameObjectBlueprint buildingBlueprint)
    {
        this.buildingBlueprint = buildingBlueprint;
    }

    public void Apply(FactionTechnology factionTechnology)
    {
        factionTechnology.UnlockBuilding(buildingBlueprint);
    }
}
