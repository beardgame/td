using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities;

sealed class Comparer<T> : IComparer<T>
{
    public static Comparer<T> Comparing<TComparable>(Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
    {
        return Comparing(selector, System.Collections.Generic.Comparer<TComparable>.Default);
    }

    public static Comparer<T> Comparing<TComparable>(Func<T, TComparable> selector, IComparer<TComparable> comparer)
    {
        return new Comparer<T>((x, y) => comparer.Compare(selector(x), selector(y)));
    }

    public static Comparer<T> Reversed(IComparer<T> comparer)
    {
        return new Comparer<T>((x, y) => comparer.Compare(y, x));
    }

    private readonly Func<T, T, int> compareFunc;

    private Comparer(Func<T, T, int> compareFunc)
    {
        this.compareFunc = compareFunc;
    }

    public int Compare(T x, T y) => compareFunc(x, y);

    public Comparer<T> ThenComparing<TComparable>(Func<T, TComparable> selector)
        where TComparable : IComparable<TComparable>
    {
        return ThenComparing(selector, System.Collections.Generic.Comparer<TComparable>.Default);
    }

    public Comparer<T> ThenComparing<TComparable>(Func<T, TComparable> selector, IComparer<TComparable> comparer)
    {
        return new Comparer<T>(func);

        int func(T x, T y)
        {
            var firstComparison = compareFunc(x, y);
            return firstComparison == 0 ? comparer.Compare(selector(x), selector(y)) : firstComparison;
        }
    }
}

static class Comparer
{
    public static Comparer<TToCompare> Comparing<TToCompare, TComparable>(Func<TToCompare, TComparable> selector)
        where TComparable : IComparable<TComparable>
    {
        return Comparer<TToCompare>.Comparing(selector);
    }

    public static Comparer<TToCompare> Comparing<TToCompare, TComparable>(
        Func<TToCompare, TComparable> selector, IComparer<TComparable> comparer)
    {
        return Comparer<TToCompare>.Comparing(selector, comparer);
    }

    public static IComparer<T> Reversed<T>(this IComparer<T> comparer)
    {
        return Comparer<T>.Reversed(comparer);
    }
}
