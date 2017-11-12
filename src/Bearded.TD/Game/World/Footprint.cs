using System.Collections.Generic;
using Bearded.TD.Tiles;

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
        });

        /*
           X #
            #
        */
        public static readonly Footprint TriangleDown = new Footprint(new[]
        {
            new Step(0, 0), new Step(Direction.Right), new Step(Direction.DownRight),
        });

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

        public Footprint(IEnumerable<Step> tileOffsets) : base(tileOffsets) { }
    }
}
