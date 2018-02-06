using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Utilities
{
    static class LinqExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source == null || !source.Any();
    }
}
