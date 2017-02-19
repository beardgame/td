using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    sealed class Footprint
    {
        /*
            X
        */
        public static Footprint Single = new Footprint(new []
        {
            new Step(0, 0),
        });

        /*
            #
           X #
        */
        public static Footprint TriangleUp = new Footprint(new []
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight),
        }, .5f * new Difference2(Constants.Game.World.HexagonWidth, Constants.Game.World.HexagonSide));

        /*
           X #
            #
        */
        public static Footprint TriangleDown = new Footprint(new []
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.DownRight),
        }, .5f * new Difference2(Constants.Game.World.HexagonWidth, -Constants.Game.World.HexagonSide));

        /*
           # #
          # X #
           # #
        */
        public static Footprint CircleSeven = new Footprint(new []
        {
            new Step(0, 0),
            new Step(Direction.Left), new Step(Direction.DownLeft), new Step(Direction.DownRight),
            new Step(Direction.Right), new Step(Direction.UpRight), new Step(Direction.UpLeft),
        });

        private readonly IEnumerable<Step> tileOffsets;
        private readonly Difference2 rootTileOffset;

        private Footprint(IEnumerable<Step> tileOffsets) : this(tileOffsets, new Difference2(0, 0))
        { }

        private Footprint(IEnumerable<Step> tileOffsets, Difference2 rootTileOffset)
        {
            this.tileOffsets = tileOffsets;
            this.rootTileOffset = rootTileOffset;
        }

        public IEnumerable<Tile<TileInfo>> OccupiedTiles(Tile<TileInfo> rootTile)
            => tileOffsets.Select(rootTile.Offset);

        public Position2 Center(Level level, Tile<TileInfo> rootTile)
        {
            return level.GetPosition(rootTile) - rootTileOffset;
        }

        public Tile<TileInfo> RootTileClosestToWorldPosition(Level level, Position2 position)
        {
            return level.GetTile(position + rootTileOffset);
        }

        public PositionedFootprint Positioned(Level level, Position2 position)
        {
            return new PositionedFootprint(level, this, RootTileClosestToWorldPosition(level, position));
        }
    }
}
