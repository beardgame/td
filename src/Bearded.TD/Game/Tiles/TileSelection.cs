using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Game.World;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Tiles
{
    abstract class TileSelection
    {
        public static TileSelection FromFootprints(FootprintGroup footprints)
            => footprints.Footprints.Count == 1
                ? new SingleSelection(footprints.Footprints[0])
                : (TileSelection)new GroupSelection(footprints);

        public abstract PositionedFootprint GetPositionedFootprint(Level level, Position2 position);

        private sealed class SingleSelection : TileSelection
        {
            private readonly Footprint footprint;

            public SingleSelection(Footprint footprint)
            {
                this.footprint = footprint;
            }

            public override PositionedFootprint GetPositionedFootprint(Level level, Position2 position)
            {
                return new PositionedFootprint(level, footprint, footprint.RootTileClosestToWorldPosition(level, position));
            }
        }

        private sealed class GroupSelection : TileSelection
        {
            private readonly ReadOnlyCollection<Footprint> footprints;

            public GroupSelection(FootprintGroup footprints)
            {
                this.footprints = footprints.Footprints;
            }

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
