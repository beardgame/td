using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.Geometry;

readonly record struct Ray3(Position3 Start, Difference3 Direction, Unit Radius)
{
    public Ray XY => new (Start.XY(), Direction.XY());

    public Ray3(Position3 start, Position3 end, Unit? radius) : this(start, end - start, radius ?? Unit.Zero)
    {
    }

    public Ray3(Position3 start, Difference3 direction) : this(start, direction, Unit.Zero)
    {
    }

    public Position3 PointAtEnd
        => Start + Direction;

    public Position3 PointAt(float rayFactor)
        => Start + Direction * rayFactor;
}
