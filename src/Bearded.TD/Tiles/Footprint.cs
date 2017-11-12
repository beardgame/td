using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    class Footprint<TTileInfo>
    {
        private readonly IEnumerable<Step> tileOffsets;
        private readonly Difference2 rootTileOffset;

        public Footprint(IEnumerable<Step> tileOffsets)
            : this(tileOffsets, new Difference2(0, 0))
        { }

        public Footprint(IEnumerable<Step> tileOffsets, Difference2 rootTileOffset)
        {
            this.tileOffsets = tileOffsets;
            this.rootTileOffset = rootTileOffset;
        }

        public IEnumerable<Tile<TTileInfo>> OccupiedTiles(Tile<TTileInfo> rootTile)
            => tileOffsets.Select(rootTile.Offset);

        public Position2 Center(Level<TTileInfo> level, Tile<TTileInfo> rootTile)
        {
            return level.GetPosition(rootTile) - rootTileOffset;
        }

        public Tile<TTileInfo> RootTileClosestToWorldPosition(Level<TTileInfo> level, Position2 position)
        {
            return level.GetTile(position + rootTileOffset);
        }
    }
}
