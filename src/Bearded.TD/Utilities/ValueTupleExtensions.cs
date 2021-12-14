using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities;

static class ValueTupleExtensions
{
    public static IEnumerable<T> Enumerate<T>(this ValueTuple<T, T, T> tuple)
    {
        yield return tuple.Item1;
        yield return tuple.Item2;
        yield return tuple.Item3;
    }

    public static bool AnyIsNull<T1, T2>(this ValueTuple<T1, T2> values)
        => values.Item1 == null || values.Item2 == null;
    public static bool AnyIsNull<T1, T2, T3>(this ValueTuple<T1, T2, T3> values)
        => values.Item1 == null || values.Item2 == null || values.Item3 == null;
    public static bool AnyIsNull<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3, T4> values)
        => values.Item1 == null || values.Item2 == null || values.Item3 == null || values.Item4 == null;

    public static bool AnyIsNotNull<T1, T2>(this ValueTuple<T1, T2> values)
        => values.Item1 != null || values.Item2 != null;
    public static bool AnyIsNotNull<T1, T2, T3>(this ValueTuple<T1, T2, T3> values)
        => values.Item1 != null || values.Item2 != null || values.Item3 != null;
    public static bool AnyIsNotNull<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3, T4> values)
        => values.Item1 != null || values.Item2 != null || values.Item3 != null || values.Item4 != null;

    public static bool AllAreNull<T1, T2>(this ValueTuple<T1, T2> values)
        => values.Item1 == null && values.Item2 == null;
    public static bool AllAreNull<T1, T2, T3>(this ValueTuple<T1, T2, T3> values)
        => values.Item1 == null && values.Item2 == null && values.Item3 == null;
    public static bool AllAreNull<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3, T4> values)
        => values.Item1 == null && values.Item2 == null && values.Item3 == null && values.Item4 == null;

    public static bool NoneAreNull<T1, T2>(this ValueTuple<T1, T2> values)
        => values.Item1 != null && values.Item2 != null;
    public static bool NoneAreNull<T1, T2, T3>(this ValueTuple<T1, T2, T3> values)
        => values.Item1 != null && values.Item2 != null && values.Item3 != null;
    public static bool NoneAreNull<T1, T2, T3, T4>(this ValueTuple<T1, T2, T3, T4> values)
        => values.Item1 != null && values.Item2 != null && values.Item3 != null && values.Item4 != null;
}