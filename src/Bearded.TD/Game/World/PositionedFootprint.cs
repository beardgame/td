using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    readonly struct PositionedFootprint
    {
        public Tile RootTile { get; }
        public FootprintGroup Footprint { get; }
        public int FootprintIndex { get; }

        public Angle Orientation => Footprint.Orientations[FootprintIndex];
        public IEnumerable<Tile> OccupiedTiles => Footprint?.Footprints[FootprintIndex].OccupiedTiles(RootTile);
        public Position2 CenterPosition => Footprint?.Footprints[FootprintIndex].Center(RootTile) ?? new Position2();

        public PositionedFootprint(FootprintGroup footprint, int index, Tile rootTile)
        {
            RootTile = rootTile;
            Footprint = footprint;
            FootprintIndex = index;
        }

        public bool IsValid(Level level) => OccupiedTiles?.All(level.IsValid) ?? false;
    }
}
