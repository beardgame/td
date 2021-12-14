using System.Collections.Generic;

namespace Bearded.TD.Utilities;

static class DeconstructionExtensions
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
    {
        key = source.Key;
        value = source.Value;
    }
}