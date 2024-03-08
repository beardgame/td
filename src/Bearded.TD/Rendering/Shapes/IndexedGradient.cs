using System;
using Bearded.TD.UI.Shapes;

namespace Bearded.TD.Rendering.Shapes;

readonly struct IndexedGradientStop(ushort remainingCount, GradientStop stop)
{
    // 2 bytes reserved for things like interpolation mode
    private readonly uint remainingCount2reserved2 = remainingCount;
    private readonly GradientStop stop = stop;
}

readonly ref struct IndexedGradient(int count, ReadOnlySpan<IndexedGradientStop> stops)
{
    public int Count { get; } = count;
    public ReadOnlySpan<IndexedGradientStop> Stops { get; } = stops;

    public static IndexedGradient From(Gradient gradient)
        => From(gradient.Stops);

    public static IndexedGradient From(ReadOnlySpan<GradientStop> stops)
    {
        if(stops.Length > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(stops), $"Not supporting more than {ushort.MaxValue} stops");

        var buffer = new IndexedGradientStop[stops.Length];
        for (var i = 0; i < stops.Length; i++)
        {
            buffer[i] = new IndexedGradientStop((ushort)(stops.Length - i - 1), stops[i]);
        }
        return new IndexedGradient(stops.Length, buffer);
    }

    public static IndexedGradient From(ReadOnlySpan<GradientStop> stops, Span<IndexedGradientStop> buffer)
    {
        if (stops.Length > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(stops), $"Not supporting more than {ushort.MaxValue} stops");
        if (stops.Length > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(buffer), $"Buffer too small for {stops.Length} stops");

        for (var i = 0; i < stops.Length; i++)
        {
            buffer[i] = new IndexedGradientStop((ushort)(stops.Length - i), stops[i]);
        }
        return new IndexedGradient(stops.Length, buffer);
    }
}
