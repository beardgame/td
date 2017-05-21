using System.Linq;
using Bearded.TD.Game.World;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    abstract class TileSelection
    {
        public static TileSelection Single => FromFootprint(Footprint.Single);
        public static TileSelection Triangle = new TriangleSelection();

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
            private static readonly Footprint[] footprints = { Footprint.TriangleDown, Footprint.TriangleUp };

            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                var (footprint, root, _) = footprints.Select(f =>
                    {
                        var bestRoot = f.RootTileClosestToWorldPosition(level, position);
                        var center = f.Center(level, bestRoot);
                        var distance = (center - position).LengthSquared;
                        return (f, bestRoot, Distance: distance);
                    })
                    .MinBy(r => r.Distance.NumericValue);

                return new PositionedFootprint(level, footprint, root);
            }
        }
    }
}
