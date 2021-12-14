using OpenTK.Mathematics;

namespace Bearded.TD.Utilities.Geometry;

struct Line
{
    public Vector2 Start { get; }
    public Vector2 Direction { get; }

    private Line(Vector2 start, Vector2 direction)
    {
        Start = start;
        Direction = direction;
    }

    public static Line Between(Vector2 a, Vector2 b)
        => new Line(a, b - a);
    public static Line Between(float x1, float y1, float x2, float y2)
        => new Line(new Vector2(x1, x2), new Vector2(x2 - x1, y2 - y1));

    public bool IntersectsAsSegments(Line other)
    {
        var a = Start;
        var b = Direction;
        var c = other.Start;
        var d = other.Direction;

        var denominator = b.Y * d.X - b.X * d.Y;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (denominator == 0)
            return false;

        var xDif = a.X - c.X;
        var yDif = c.Y - a.Y;

        var thisNominator = d.Y * xDif + d.X * yDif;
        var thisF = thisNominator / denominator;

        if (thisF <= 0 || thisF >= 1)
            return false;

        var otherNominator = b.Y * xDif + b.X * yDif;
        var otherF = otherNominator / denominator;

        return otherF > 0 && otherF < 1;
    }
}