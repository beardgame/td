using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints
{
    interface ITileOccupation
    {
        IEnumerable<Tile> OccupiedTiles { get; }
    }
}
