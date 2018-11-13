using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities
{
    sealed class Comparer<T> : IComparer<T>
    {
        public static Comparer<TToCompare> Comparing<TToCompare, TComparable>(Func<TToCompare, TComparable> selector)
            where TComparable : IComparable<TComparable>
        {
            return new Comparer<TToCompare>((x, y) => selector(x).CompareTo(selector(y)));
        }

        private readonly Func<T, T, int> compareFunc;

        private Comparer(Func<T, T, int> compareFunc)
        {
            this.compareFunc = compareFunc;
        }

        public int Compare(T x, T y) => compareFunc(x, y);
    }
}
