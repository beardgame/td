using System;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    abstract class TileSelection
    {
        public static TileSelection Single => FromFootprint(Footprint.Single);
        public static TileSelection FromFootprint(Footprint footprint) => new FootprintSelection(footprint);

        public abstract PositionedFootprint GetPositionedFootprint(Level level, Position2 position);

        private class FootprintSelection : TileSelection
        {
            private readonly Footprint footprint;

            public FootprintSelection(Footprint footprint)
            {
                this.footprint = footprint;
            }

            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                return new PositionedFootprint(level, footprint, footprint.RootTileClosestToWorldPosition(level, position));
            }
        }

        private class TriangleSelection : TileSelection
        {
            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                throw new NotImplementedException();
            }
        }
    }
}
