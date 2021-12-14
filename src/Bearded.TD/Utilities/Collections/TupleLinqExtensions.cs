using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Utilities.Collections;

static class TupleLinqExtensions
{
    public static IEnumerable<(T, T)> WhereBoth<T>(this IEnumerable<(T, T)> enumerable, Func<T, bool> predicate)
    {
        return enumerable.Where(tuple => predicate(tuple.Item1) && predicate(tuple.Item2));
    }

    public static IEnumerable<(TOut, TOut)> SelectBoth<TIn, TOut>(
        this IEnumerable<(TIn, TIn)> enumerable, Func<TIn, TOut> func)
    {
        return enumerable.Select(tuple => (func(tuple.Item1), func(tuple.Item2)));
    }
}