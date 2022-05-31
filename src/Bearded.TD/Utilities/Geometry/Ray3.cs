using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.Geometry;

readonly struct Ray3
{
    public Position3 Start { get; }
    public Difference3 Direction { get; }

    public Ray XY => new (Start.XY(), Direction.XY());

    public Ray3(Position3 start, Position3 end) : this(start, end - start)
    {
    }

    public Ray3(Position3 start, Difference3 direction)
    {
        Start = start;
        Direction = direction;
    }

    public Position3 PointAtEnd
        => Start + Direction;

    public Position3 PointAt(float rayFactor)
        => Start + Direction * rayFactor;
}
