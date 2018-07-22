using Bearded.TD.Game.Factions;

namespace Bearded.TD.Game.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
        IBuildingBlueprint Blueprint { get; }
        Faction Faction { get; }
        int Health { get; }
    }
}
