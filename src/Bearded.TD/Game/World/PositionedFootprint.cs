using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Mods.Models;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct PositionedFootprint
    {
        private readonly Level level;
        public Tile<TileInfo> RootTile { get; }
        public FootprintGroup Footprint { get; }
        public int FootprintIndex { get; }

        public PositionedFootprint(Level level, FootprintGroup footprint, int index, Tile<TileInfo> rootTile)
        {
            this.level = level;
            RootTile = rootTile;
            Footprint = footprint;
            FootprintIndex = index;
        }

        public IEnumerable<Tile<TileInfo>> OccupiedTiles => Footprint?.Footprints[FootprintIndex].OccupiedTiles(RootTile);
        public bool IsValid => OccupiedTiles?.All(tile => tile.IsValid) ?? false;
        public Position2 CenterPosition => Footprint?.Footprints[FootprintIndex].Center(level, RootTile) ?? new Position2();
    }
}
