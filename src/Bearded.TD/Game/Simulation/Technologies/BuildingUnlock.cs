using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Technologies;

sealed class BuildingUnlock(IGameObjectBlueprint buildingBlueprint) : ITechnologyUnlock
{
    private readonly IObjectAttributes attributes = buildingBlueprint.AttributesOrDefault();

    public string Description => $"Unlock building: {attributes.Name}";

    public void Apply(FactionTechnology factionTechnology)
    {
        factionTechnology.UnlockBuilding(buildingBlueprint);
    }
}
