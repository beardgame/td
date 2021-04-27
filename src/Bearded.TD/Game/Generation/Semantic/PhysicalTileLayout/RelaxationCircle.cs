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
        public LogicalNode Logical { get; }

        public Node(Position2 position, Unit radius, LogicalNode logical)
            : base(position, radius)
        {
            Logical = logical;
        }
    }
}
