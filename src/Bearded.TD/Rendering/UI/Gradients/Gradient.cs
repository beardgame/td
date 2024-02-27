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
}
