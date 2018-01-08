using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    class Footprint<TTileInfo>
    {
        private readonly IEnumerable<Step> tileOffsets;
        private readonly Difference2 rootTileOffset; // vector that points from center of footprint to center of root tile
        
        public Footprint(IEnumerable<Step> tileOffsets)
        {
            var steps = tileOffsets as IList<Step> ?? tileOffsets.ToList();
            this.tileOffsets = steps;
            rootTileOffset = -steps
                    .Select(step =>
                        step.X * Constants.Game.World.HexagonGridUnitX + step.Y * Constants.Game.World.HexagonGridUnitY)
                    .Aggregate((diff1, diff2) => diff1 + diff2) / steps.Count;
        }

        public IEnumerable<Tile<TTileInfo>> OccupiedTiles(Tile<TTileInfo> rootTile)
            => tileOffsets.Select(rootTile.Offset);

        public Position2 Center(Level<TTileInfo> level, Tile<TTileInfo> rootTile)
            => level.GetPosition(rootTile) - rootTileOffset;

        public Tile<TTileInfo> RootTileClosestToWorldPosition(Level<TTileInfo> level, Position2 position)
            => level.GetTile(position + rootTileOffset);
    }
}
