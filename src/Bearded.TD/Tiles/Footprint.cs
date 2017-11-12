using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Tiles
{
    sealed class Footprint : IBlueprint
    {
        /*
            X
        */
        public static readonly Footprint Single = new Footprint("single", new[]
        {
            new Step(0, 0)
        });

        /*
            #
           X #
        */
        public static readonly Footprint TriangleUp = new Footprint("triangle_up", new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight),
        }, .5f * new Difference2(-Constants.Game.World.HexagonWidth, -Constants.Game.World.HexagonSide));

        /*
           X #
            #
        */
        public static readonly Footprint TriangleDown = new Footprint("triangle_down", new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.DownRight),
        }, .5f * new Difference2(-Constants.Game.World.HexagonWidth, Constants.Game.World.HexagonSide));

        /*
           # #
          # X #
           # #
        */
        public static readonly Footprint CircleSeven = new Footprint("circle_seven", new[]
        {
            new Step(0, 0),
            new Step(Direction.Left), new Step(Direction.DownLeft), new Step(Direction.DownRight),
            new Step(Direction.Right), new Step(Direction.UpRight), new Step(Direction.UpLeft),
        });

        /*
           #
          X #
           #
        */
        public static readonly Footprint DiamondTopBottom = new Footprint("diamond_top_bottom", new[]
        {
            new Step(0, 0), new Step(Direction.DownRight), new Step(Direction.Right), new Step(Direction.UpRight)
        }, -.5f * new Difference2(Direction.Right.Vector()));

        /*
            X #
           # #
        */
        public static readonly Footprint DiamondBottomLeftTopRight = new Footprint("diamond_bottom_left_top_right", new[]
        {
            new Step(0, 0), new Step(Direction.DownLeft), new Step(Direction.DownRight), new Step(Direction.Right)
        }, -.5f * new Difference2(Direction.DownRight.Vector()));

        /*
           # #
            X #
        */
        public static readonly Footprint DiamondTopLeftBottomRight = new Footprint("diamond_top_left_bottom_right", new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight), new Step(Direction.UpLeft)
        }, -.5f * new Difference2(Direction.UpRight.Vector()));

        /*
            #
           X
        */
        public static readonly Footprint LineUp = new Footprint("line_up", new[]
        {
            new Step(0, 0), new Step(Direction.UpRight)
        }, -.5f * new Difference2(Direction.UpRight.Vector()));

        /*
          X #
        */
        public static readonly Footprint LineStraight = new Footprint("line_straight", new[]
        {
            new Step(0, 0), new Step(Direction.Right)
        }, -.5f * new Difference2(Direction.Right.Vector()));

        /*
           X
            #
        */
        public static readonly Footprint LineDown = new Footprint("line_down", new[]
        {
            new Step(0, 0), new Step(Direction.DownRight)
        }, -.5f * new Difference2(Direction.DownRight.Vector()));
        
        public string Name { get; }
        private readonly IEnumerable<Step> tileOffsets;
        private readonly Difference2 rootTileOffset;

        private Footprint(string name, IEnumerable<Step> tileOffsets)
            : this(name, tileOffsets, new Difference2(0, 0))
        { }

        private Footprint(string name, IEnumerable<Step> tileOffsets, Difference2 rootTileOffset)
        {
            this.tileOffsets = tileOffsets;
            this.rootTileOffset = rootTileOffset;
            Name = name;
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
