using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct PositionedFootprint
    {
        private readonly Level level;
        public Tile RootTile { get; }
        public FootprintGroup Footprint { get; }
        public int FootprintIndex { get; }

        public PositionedFootprint(Level level, FootprintGroup footprint, int index, Tile rootTile)
        {
            this.level = level;
            RootTile = rootTile;
            Footprint = footprint;
            FootprintIndex = index;
        }

        public IEnumerable<Tile> OccupiedTiles => Footprint?.Footprints[FootprintIndex].OccupiedTiles(RootTile);
        public bool IsValid => OccupiedTiles?.All(level.IsValid) ?? false;
        public Position2 CenterPosition => Footprint?.Footprints[FootprintIndex].Center(level, RootTile) ?? new Position2();
    }
}
