using System.Collections.Generic;
using Bearded.TD.Game.Factions;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
        IBuildingBlueprint Blueprint { get; }
        Faction Faction { get; }
        int Health { get; }
        IEnumerable<Tile> OccupiedTiles { get; }
    }
}
