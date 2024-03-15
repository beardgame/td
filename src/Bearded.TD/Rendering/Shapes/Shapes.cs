using Bearded.Graphics;
using Bearded.TD.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

readonly record struct Rectangle(Vector3d TopLeft, Vector2d Size, double CornerRadius);
readonly record struct Circle(Vector3d Center, double Radius);
readonly record struct Hexagon(Vector3d Center, double Radius, double CornerRadius);
readonly record struct Shadow(Vector3d Offset, double PenumbraRadius, Color Color);

static class Shapes
{
    public static Rectangle Rectangle(Vector3d topLeft, Vector2d size, double cornerRadius = 0)
        => new(topLeft, size, cornerRadius);

    public static Circle Circle(Vector3d center, double radius)
        => new(center, radius);

    public static Hexagon Hexagon(Vector3d center, double radius, double cornerRadius = 0)
        => new(center, radius, cornerRadius);

    public static Shadow Shadow(Vector3d offset, double blurRadius, Color color)
        => new(offset, blurRadius, color);

    public static Rectangle Rectangle(Vector2d topLeft, Vector2d size, double cornerRadius = 0)
        => new(topLeft.WithZ(), size, cornerRadius);

    public static Circle Circle(Vector2d center, double radius)
        => new(center.WithZ(), radius);

    public static Hexagon Hexagon(Vector2d center, double radius, double cornerRadius = 0)
        => new(center.WithZ(), radius, cornerRadius);

    public static Shadow Shadow(Vector2d offset, double blurRadius, Color color)
        => new(offset.WithZ(), blurRadius, color);
}
