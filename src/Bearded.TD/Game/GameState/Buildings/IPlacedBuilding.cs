using System.Collections.Generic;
using Bearded.TD.Game.GameState.Factions;
using Bearded.TD.Game.Meta;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.GameState.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
        IBuildingBlueprint Blueprint { get; }
        Faction Faction { get; }
        IEnumerable<Tile> OccupiedTiles { get; }

        event VoidEventHandler? Deleting;
    }
}
