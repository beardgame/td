using System;
using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD
{
    internal static class EnumerableProxy
    {
        public static EnumerableProxy<T> AsReadOnlyEnumerable<T>(this IEnumerable<T> enumerable)
            => new EnumerableProxy<T>(enumerable);
    }

    internal struct EnumerableProxy<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> list;

        public EnumerableProxy(IEnumerable<T> list)
        {
            this.list = list;
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}