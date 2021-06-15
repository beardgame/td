using System.Collections.Generic;
using Bearded.TD.Game.Meta;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IPlacedBuilding : ISelectable
    {
        IBuildingState State { get; }

        IBuildingBlueprint Blueprint { get; }
        Faction Faction { get; }
        IEnumerable<Tile> OccupiedTiles { get; }

        IEnumerable<T> GetComponents<T>();

        event VoidEventHandler? Deleting;
    }
}
