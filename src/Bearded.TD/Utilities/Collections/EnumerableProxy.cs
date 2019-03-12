using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD.Utilities.Collections
{
    static class EnumerableProxy
    {
        public static EnumerableProxy<T> AsReadOnlyEnumerable<T>(this IEnumerable<T> enumerable)
            => new EnumerableProxy<T>(enumerable);
    }

    struct EnumerableProxy<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> list;

        public EnumerableProxy(IEnumerable<T> list)
        {
            this.list = list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

    }
}
