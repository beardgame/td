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
}