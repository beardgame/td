using System;
using Bearded.Graphics;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

readonly record struct Rectangle(Vector3d TopLeft, Vector2d Size, double CornerRadius);
readonly record struct Circle(Vector3d Center, double Radius);
readonly record struct Hexagon(Vector3d Center, double Radius, double CornerRadius);
readonly record struct Shadow(Vector3d Offset, double PenumbraRadius, Color Color);

readonly record struct HexGrid(Tile Origin, HexGridBitField Bits, float CornerRadius);

readonly struct HexGridBitField(int value)
{
    public static HexGridBitField From(Tile origin, Func<Tile, bool> predicate)
    {
        var bits = 0;

        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                bits |= predicate(origin + new Step(x, y)) ? 1 << (x + y * 4) : 0;
            }
        }

        return new HexGridBitField(bits);
    }

    public float ToFloatBits()
    {
        return BitConverter.Int32BitsToSingle(value);
    }

    public bool IsEmpty => value == 0;
}

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

    public static HexGrid HexGrid(Tile origin, HexGridBitField bits, float cornerRadius)
        => new(origin, bits, cornerRadius);
}
