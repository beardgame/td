using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Buildings
{
    interface IBuildingFootprint
    {
        IEnumerable<Tile> OccupiedTiles { get; }
    }
}
