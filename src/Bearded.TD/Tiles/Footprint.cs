using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    sealed class Footprint
    {
        public static readonly Footprint Single = new(new[] { new Step(0, 0) });

        private readonly ImmutableArray<Step> tileOffsets;
        private readonly Difference2 rootTileOffset; // vector that points from center of footprint to center of root tile

        public Footprint(IEnumerable<Step> tileOffsets)
        {
            this.tileOffsets = tileOffsets.ToImmutableArray();
            rootTileOffset = -this.tileOffsets
                    .Select(step =>
                        step.X * Constants.Game.World.HexagonGridUnitX + step.Y * Constants.Game.World.HexagonGridUnitY)
                    .Aggregate((diff1, diff2) => diff1 + diff2) / this.tileOffsets.Length;
        }

        public IEnumerable<Tile> OccupiedTiles(Tile rootTile) => tileOffsets.Select(rootTile.Offset);

        public Position2 Center(Tile rootTile) => Level.GetPosition(rootTile) - rootTileOffset;

        public Tile RootTileClosestToWorldPosition(Position2 position) => Level.GetTile(position + rootTileOffset);
    }
}
