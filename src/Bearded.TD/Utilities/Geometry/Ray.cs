using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.Geometry
{
    struct Ray
    {
        public Position2 Start { get; }
        public Difference2 Direction { get; }

        public Ray(Position2 start, Difference2 direction)
        {
            Start = start;
            Direction = direction;
        }
    }
}