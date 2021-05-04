using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class RelaxationCircle
    {
        public Position2 Position { get; set; }
        public Unit Radius { get; }

        public RelaxationCircle(Position2 position, Unit radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}
