using System.Collections.Generic;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Interaction
{
    static class ClickHandlerExtensions
    {
        public static IEnumerable<Tile<TileInfo>> OccupiedTiles(
                this IClickHandler clickHandler, Tile<TileInfo> rootTile)
            => clickHandler.Footprint.OccupiedTiles(rootTile);
    }
}
