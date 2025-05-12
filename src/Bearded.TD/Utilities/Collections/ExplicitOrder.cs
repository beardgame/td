using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using static System.Collections.Generic.Comparer<int>;

namespace Bearded.TD.Utilities.Collections;

sealed class ExplicitOrder<T> : IComparer<T> where T : notnull
{
    private readonly ImmutableDictionary<T, int> lookup;

    internal ExplicitOrder(ImmutableDictionary<T, int> lookup)
    {
        this.lookup = lookup;
    }

    public int Compare(T? x, T? y)
    {
        var i = 0;
        var j = 0;

        if (x != null && !lookup.TryGetValue(x, out i))
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }
        if (y != null && !lookup.TryGetValue(y, out j))
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        return Default.Compare(i, j);
    }
}

static class ExplicitOrder
{
    public static ExplicitOrder<T> Create<T>(IList<T> list) where T : notnull
    {
        return new ExplicitOrder<T>(list.Indexed().ToImmutableDictionary(t => t.Item1, t => t.Item2));
    }
}
