using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints;

delegate void TileEventHandler(Tile tile);

interface ITilePresence
{
    event TileEventHandler TileAdded;
    event TileEventHandler TileRemoved;

    IEnumerable<Tile> OccupiedTiles { get; }
}
