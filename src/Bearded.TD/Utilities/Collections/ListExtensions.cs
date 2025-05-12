using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities.Collections;

static class ListExtensions
{
    public static T Shift<T>(this IList<T> list)
    {
        if (list == null) throw new ArgumentNullException(nameof(list));
        if (list.Count == 0) throw new ArgumentException(nameof(list));

        var elmt = list[0];
        list.RemoveAt(0);
        return elmt;
    }

    /// <summary>
    /// Allocation-free version of List.RemoveAll, assuming the predicate is a static function.
    /// </summary>
    public static int RemoveAll<T, TContext>(this List<T> list, TContext context, Func<TContext, T, bool> match)
    {
        // Implementation adapted from system method.

        var freeIndex = 0;
        var count = list.Count;

        while (freeIndex < count && !match(context, list[freeIndex]))
            freeIndex++;

        if (freeIndex >= count)
            return 0;

        var current = freeIndex + 1;
        while (current < count)
        {
            while (current < count && match(context, list[current]))
                current++;

            if (current < count)
                list[freeIndex++] = list[current++];
        }

        var removedCount = count - freeIndex;
        list.RemoveRange(freeIndex, removedCount);

        return removedCount;
    }
}
