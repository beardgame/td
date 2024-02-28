using System;
using Bearded.Graphics;

namespace Bearded.TD.Rendering.UI.Gradients;

readonly struct GradientStop(float position, Color color)
{
    public readonly float Position = position;
    public readonly Color Color = color;

    public static implicit operator GradientStop((float position, Color color) tuple)
        => new(tuple.position, tuple.color);
}

readonly ref struct Gradient(ReadOnlySpan<GradientStop> stops)
{
    public ReadOnlySpan<GradientStop> Stops { get; } = stops;

    public Color Lerp(float t)
    {
        return Stops.Length switch
        {
            0 => default,
            1 => Stops[0].Color,
            _ when t <= Stops[0].Position => Stops[0].Color,
            _ when t >= Stops[^1].Position => Stops[^1].Color,
            _ => lerp(t),
        };
    }

    private Color lerp(float position)
    {
        for (var i = 1; i < Stops.Length; i++)
        {
            if (position < Stops[i].Position)
            {
                var t = (position - Stops[i - 1].Position) / (Stops[i].Position - Stops[i - 1].Position);
                return Color.Lerp(Stops[i - 1].Color, Stops[i].Color, t);
            }
        }

        throw new InvalidOperationException("This should never happen.");
    }
}
