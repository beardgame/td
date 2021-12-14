using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Technologies;

sealed class BuildingUnlock : ITechnologyUnlock
{
    private readonly IComponentOwnerBlueprint buildingBlueprint;

    public string Description => $"Unlock building: {buildingBlueprint.GetName()}";

    public BuildingUnlock(IComponentOwnerBlueprint buildingBlueprint)
    {
        this.buildingBlueprint = buildingBlueprint;
    }

    public void Apply(FactionTechnology factionTechnology)
    {
        factionTechnology.UnlockBuilding(buildingBlueprint);
    }
}