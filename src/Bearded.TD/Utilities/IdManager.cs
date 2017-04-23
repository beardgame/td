using System;
using System.Collections.Generic;

namespace Bearded.TD.Utilities
{
    sealed class IdManager
    {
        private readonly Dictionary<Type, int> lastIds = new Dictionary<Type, int>();

        public Id<T> GetNext<T>()
        {
            var type = typeof(T);
            var i = lastIds.ValueOrDefault(type) + 1;
            lastIds[type] = i;
            return new Id<T>(i);
        }
    }
}