using System;

namespace Bearded.TD.Utilities.Collections;

static class StructSpanExtensions
{
    public static T? MinByOrDefault<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
        where T : struct
    {
        return span.IsEmpty ? null : span.MinBy(selector);
    }

    public static T? MaxByOrDefault<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
        where T : struct
    {
        return span.IsEmpty ? null : span.MaxBy(selector);
    }
}

static class ClassSpanExtensions
{
    public static T? MinByOrDefault<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
        where T : class
    {
        return span.IsEmpty ? null : span.MinBy(selector);
    }

    public static T? MaxByOrDefault<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
        where T : class
    {
        return span.IsEmpty ? null : span.MaxBy(selector);
    }
}

static class SpanExtensions
{
    public static T MinBy<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span is empty", nameof(span));

        var minItem = span[0];
        var minValue = selector(minItem);

        for (var i = 1; i < span.Length; i++)
        {
            var item = span[i];
            var value = selector(item);
            if (value.CompareTo(minValue) < 0)
            {
                minItem = item;
                minValue = value;
            }
        }

        return minItem;
    }
    public static T MaxBy<T, TComparable>(this ReadOnlySpan<T> span, Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
    {
        if (span.IsEmpty)
            throw new ArgumentException("Span is empty", nameof(span));

        var maxItem = span[0];
        var maxValue = selector(maxItem);

        for (var i = 1; i < span.Length; i++)
        {
            var item = span[i];
            var value = selector(item);
            if (value.CompareTo(maxValue) > 0)
            {
                maxItem = item;
                maxValue = value;
            }
        }

        return maxItem;
    }
}
