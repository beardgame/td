using System.Collections.Generic;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    sealed class Footprint : Footprint<TileInfo>
    {
        /*
            X
        */
        public static readonly Footprint Single = new Footprint(new[]
        {
            new Step(0, 0)
        });

        /*
            #
           X #
        */
        public static readonly Footprint TriangleUp = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight),
        }, .5f * new Difference2(-Constants.Game.World.HexagonWidth, -Constants.Game.World.HexagonSide));

        /*
           X #
            #
        */
        public static readonly Footprint TriangleDown = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.DownRight),
        }, .5f * new Difference2(-Constants.Game.World.HexagonWidth, Constants.Game.World.HexagonSide));

        /*
           # #
          # X #
           # #
        */
        public static readonly Footprint CircleSeven = new Footprint(new[]
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
        public static readonly Footprint DiamondTopBottom = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.DownRight), new Step(Direction.Right), new Step(Direction.UpRight)
        }, -.5f * new Difference2(Direction.Right.Vector()));

        /*
            X #
           # #
        */
        public static readonly Footprint DiamondBottomLeftTopRight = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.DownLeft), new Step(Direction.DownRight), new Step(Direction.Right)
        }, -.5f * new Difference2(Direction.DownRight.Vector()));

        /*
           # #
            X #
        */
        public static readonly Footprint DiamondTopLeftBottomRight = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.UpRight), new Step(Direction.UpLeft)
        }, -.5f * new Difference2(Direction.UpRight.Vector()));

        /*
            #
           X
        */
        public static readonly Footprint LineUp = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.UpRight)
        }, -.5f * new Difference2(Direction.UpRight.Vector()));

        /*
          X #
        */
        public static readonly Footprint LineStraight = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right)
        }, -.5f * new Difference2(Direction.Right.Vector()));

        /*
           X
            #
        */
        public static readonly Footprint LineDown = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.DownRight)
        }, -.5f * new Difference2(Direction.DownRight.Vector()));

        public Footprint(IEnumerable<Step> tileOffsets) : base(tileOffsets) { }

        public Footprint(IEnumerable<Step> tileOffsets, Difference2 rootTileOffset)
            : base(tileOffsets, rootTileOffset) { }
    }
}
