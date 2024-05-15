using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly record struct ShapeComponent(float ZeroDistance, float OneDistance, ShapeColor Color);

static class Edge
{
    public static ShapeComponent Outer(float width, ShapeColor color) => new(0, width, color);
    public static ShapeComponent Inner(float width, ShapeColor color) => new(0, -width, color);
}

static class Glow
{
    public static ShapeComponent Outer(float width, Color color)
        => Edge.Outer(width, GradientDefinition.SimpleGlow(color));

    public static ShapeComponent OuterFilled(float width, Color color)
        => Edge.Outer(width, GradientDefinition.SimpleGlow(color).AddFlags(GradientFlags.ExtendNegative));

    public static ShapeComponent Inner(float width, Color color)
        => Edge.Inner(width, GradientDefinition.SimpleGlow(color));
}

static class Fill
{
    private const float zero = 0;
    private const float one = float.NegativeInfinity;

    public static ShapeComponent With(Color color)
        => With(GradientDefinition.Constant(color));

    public static ShapeComponent With(GradientDefinition.SingleColor color)
        => new(zero, one, color);

    public static ShapeComponent With(ShapeColor color)
        => new(zero, one, color);
}
