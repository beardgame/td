using System;
using Bearded.TD.UI.Shapes;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Shapes;

interface IGradientBuffer
{
    public GradientId AddGradient(Gradient gradient);
    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops);
}

sealed class GradientBuffer() : TextureBuffer<IndexedGradientStop>(SizedInternalFormat.Rgb32ui), IGradientBuffer
{
    public GradientId AddGradient(Gradient gradient)
        => AddGradient(gradient.Stops);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
    {
        var gradient = IndexedGradient.From(stops, stackalloc IndexedGradientStop[stops.Length]);
        return gradient.Count == 0
            ? new GradientId(0)
            : new GradientId((uint)Add(gradient.Stops));
    }
}
