using System;
using System.Collections.Generic;
using Bearded.Utilities;
using static Bearded.Utilities.Maybe;

namespace Bearded.TD.Utilities
{
    static class MaybeExtensions
    {
        public static Maybe<T> MaybeFirst<T>(this IEnumerable<T> enumerable)
        {
            switch (enumerable)
            {
                case null:
                    throw new ArgumentNullException(nameof(enumerable));
                case IList<T> sourceList:
                    if (sourceList.Count > 0)
                        return Just(sourceList[0]);
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

        public static Maybe<T> MaybeSingle<T>(this IEnumerable<T> enumerable)
        {
            switch (enumerable)
            {
                case null:
                    throw new ArgumentNullException(nameof(enumerable));
                case IList<T> sourceList:
                    if (sourceList.Count == 1)
                        return Just(sourceList[0]);
                    if (sourceList.Count > 1)
                        throw new InvalidOperationException("Enumerable has more than one element.");
                    break;
                default:
                    using (var enumerator = enumerable.GetEnumerator())
                    {
                        if (!enumerator.MoveNext())
                            break;
                        
                        var value = enumerator.Current;
                        
                        if (enumerator.MoveNext())
                            throw new InvalidOperationException("Enumerable has more than one element.");

                        return Just(value);
                    }
            }

            return Nothing;
        }
    }
}
