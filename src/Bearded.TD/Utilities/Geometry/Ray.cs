using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.Geometry
{
    readonly struct Ray
    {
        public Position2 Start { get; }
        public Difference2 Direction { get; }

        public Ray(Position2 start, Position2 end) : this(start, end - start)
        {
        }

        public Ray(Position2 start, Difference2 direction)
        {
            Start = start;
            Direction = direction;
        }

        public Position2 PointAtEnd
            => Start + Direction;

        public Position2 PointAt(float rayFactor)
            => Start + Direction * rayFactor;
    }
}
