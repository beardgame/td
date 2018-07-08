using Bearded.TD.Game.Factions;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
        BuildingBlueprint Blueprint { get; }
        Faction Faction { get; }
    }
}
