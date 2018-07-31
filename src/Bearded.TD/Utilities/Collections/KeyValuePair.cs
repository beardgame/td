using System.Collections.Generic;

namespace Bearded.TD.Utilities.Collections
{
    static class KeyValuePair
    {
        public static KeyValuePair<TKey, TValue> From<TKey, TValue>(TKey key, TValue value)
             => new KeyValuePair<TKey, TValue>(key, value);
    }
}
