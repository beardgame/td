using Bearded.Graphics;

namespace Bearded.TD.UI.Shapes;

readonly record struct ShapeComponents(
    Fill Fill = default,
    Edge Edge = default,
    Glow OuterGlow = default,
    Glow InnerGlow = default
);

record struct Edge(float OuterWidth, float InnerWidth, ShapeColor Color)
{
    public static Edge Outer(float width, ShapeColor color) => new(width, 0, color);

    public static Edge Inner(float width, ShapeColor color) => new(0, width, color);
}

record struct Glow(float Width, ShapeColor Color)
{
    public static Glow Simple(float width, Color color) => new(width, GradientDefinition.SimpleGlow(color));

    public static implicit operator Glow((float Width, Color Color) glow) => Simple(glow.Width, glow.Color);
}

record struct Fill(ShapeColor Color)
{
    public static Fill From(ShapeColor color) => new(color);

    public static implicit operator Fill(GradientDefinition.SingleColor gradient) => new(gradient);

    public static implicit operator Fill(Color color) => new(GradientDefinition.Constant(color));
}
