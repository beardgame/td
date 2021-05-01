using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    class RelaxationCircle
    {
        public Position2 Position { get; set; }
        public Unit Radius { get; }

        public RelaxationCircle(Position2 position, Unit radius)
        {
            Position = position;
            Radius = radius;
        }
    }

    class Node : RelaxationCircle
    {
        public PlacedNode Logical { get; }

        public Node(Position2 position, Unit radius, PlacedNode logical)
            : base(position, radius)
        {
            Logical = logical;
        }
    }
}
