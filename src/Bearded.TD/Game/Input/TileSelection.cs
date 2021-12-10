using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Input
{
    abstract class TileSelection
    {
        public static TileSelection FromFootprints(FootprintGroup footprints)
            => footprints.Footprints.Length == 1
                ? new SingleSelection(footprints)
                : new GroupSelection(footprints);

        public static TileSelection Single { get; } = new SingleSelection(FootprintGroup.Single);

        public abstract PositionedFootprint GetPositionedFootprint(Position2 position);

        private sealed class SingleSelection : TileSelection
        {
            private readonly FootprintGroup footprintGroup;

            public SingleSelection(FootprintGroup footprintGroup)
            {
                this.footprintGroup = footprintGroup;
            }

            public override PositionedFootprint GetPositionedFootprint(Position2 position)
            {
                var footprint = footprintGroup.Footprints[0];

                return new PositionedFootprint(footprintGroup, 0, footprint.RootTileClosestToWorldPosition(position));
            }
        }

        private sealed class GroupSelection : TileSelection
        {
            private readonly FootprintGroup footprints;

            public GroupSelection(FootprintGroup footprints)
            {
                this.footprints = footprints;
            }

            public override PositionedFootprint GetPositionedFootprint(Position2 position)
            {
                var (root, _, index) = footprints.Footprints.Select((f, i) =>
                    {
                        var bestRoot = f.RootTileClosestToWorldPosition(position);
                        var center = f.Center(bestRoot);
                        var distance = (center - position).LengthSquared;
                        return (bestRoot, Distance: distance, index: i);
                    })
                    .MinBy(r => r.Distance.NumericValue);

                return new PositionedFootprint(footprints, index, root);
            }
        }
    }
}
