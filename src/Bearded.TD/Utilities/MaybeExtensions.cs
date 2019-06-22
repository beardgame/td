using System;
using System.Collections.Generic;
using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Utilities
{
    static class MaybeExtensions
    {
        public static Maybe<T> FirstMaybe<T>(this IEnumerable<T> enumerable)
        {
            switch (enumerable)
            {
                case null:
                    throw new ArgumentNullException(nameof(enumerable));
                case IList<T> sourceList:
                    if (sourceList.Count > 0) return Just(sourceList[0]);
                    break;
                default:
                    using (var enumerator = enumerable.GetEnumerator())
                    {
                        if (enumerator.MoveNext()) return Just(enumerator.Current);
                    }
                    break;
            }

            return Nothing;
        }
    }
}
