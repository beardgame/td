using OpenTK.Mathematics;

namespace Bearded.TD.Content.Models.Fonts;

readonly record struct Bounds(
    float Left,
    float Bottom,
    float Right,
    float Top)
{
    public Bounds Multiply(float x, float y)
    {
        return new Bounds(
            Left * x,
            Bottom * y,
            Right * x,
            Top * y);
    }

    public Bounds Translate(float x, float y)
    {
        return new Bounds(
            Left + x,
            Bottom + y,
            Right + x,
            Top + y);
    }

    public Bounds TranslateX(float x)
    {
        return this with { Left = Left + x, Right = Right + x };
    }

    public Vector2 TopLeft => new(Left, Top);
    public Vector2 TopRight => new(Right, Top);
    public Vector2 BottomLeft => new(Left, Bottom);
    public Vector2 BottomRight => new(Right, Bottom);

    public float Width => Right - Left;
    public float Height => Top - Bottom;
};
