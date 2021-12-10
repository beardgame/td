using System;

namespace Bearded.TD.Utilities;

public struct ViewportSize : IEquatable<ViewportSize>
{
    public int Width { get; }
    public int Height { get; }
    public int ScaledWidth { get; }
    public int ScaledHeight { get; }
    public float AspectRatio { get; }

    public ViewportSize(int width, int height, float uiScale = 1f)
    {
        Width = width;
        Height = height;
        ScaledWidth = (int) (width / uiScale);
        ScaledHeight = (int) (height / uiScale);
        AspectRatio = (float) Width / Height;
    }

    public bool Equals(ViewportSize other)
        => Width == other.Width
            && Height == other.Height
            && ScaledWidth == other.ScaledWidth
            && ScaledHeight == other.ScaledHeight;

    public override bool Equals(object obj) => obj is ViewportSize size && Equals(size);

    public override int GetHashCode()
    {
        unchecked
        {
            return (Width * 397) ^ Height;
        }
    }

    public static bool operator ==(ViewportSize left, ViewportSize right)
        => left.Equals(right);

    public static bool operator !=(ViewportSize left, ViewportSize right)
        => !left.Equals(right);
}