using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    struct PositionedFootprint
    {
        private readonly Level level;
        public Tile<TileInfo> RootTile { get; }
        public Footprint Footprint { get; }

        public PositionedFootprint(Level level, Footprint footprint, Tile<TileInfo> rootTile)
        {
            this.level = level;
            RootTile = rootTile;
            Footprint = footprint;
        }

        public IEnumerable<Tile<TileInfo>> OccupiedTiles => Footprint?.OccupiedTiles(RootTile);
        public bool IsValid => OccupiedTiles?.All((tile) => tile.IsValid) ?? false;
        public Position2 CenterPosition => Footprint?.Center(level, RootTile) ?? new Position2();
    }
}
