using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class Circle
    {
        public Position2 Center { get; set; }
        public Unit Radius { get; }

        public Circle(Position2 center, Unit radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
