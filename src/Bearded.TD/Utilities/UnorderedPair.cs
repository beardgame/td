using System;
using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD.Utilities;

struct UnorderedPair<T> : IEquatable<UnorderedPair<T>>, IEnumerable<T>
    where T : struct
{
    public T Item1 { get; }
    public T Item2 { get; }

    public UnorderedPair(T item1, T item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public bool Contains(T t) => t.Equals(Item1) || t.Equals(Item2);

    public T Other(T one)
    {
        if (one.Equals(Item1))
            return Item2;
        if (one.Equals(Item2))
            return Item1;
        throw new InvalidOperationException("Can only get other item if the given one is present.");
    }

    public bool Equals(UnorderedPair<T> other)
    {
        return Item1.Equals(other.Item1) && Item2.Equals(other.Item2)
            || Item1.Equals(other.Item2) && Item2.Equals(other.Item1);
    }

    public IEnumerator<T> GetEnumerator()
    {
        yield return Item1;
        yield return Item2;
    }

    public override bool Equals(object obj) => obj is UnorderedPair<T> other && Equals(other);

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Item1)
            ^ EqualityComparer<T>.Default.GetHashCode(Item2);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static bool operator ==(UnorderedPair<T> left, UnorderedPair<T> right) => left.Equals(right);
    public static bool operator !=(UnorderedPair<T> left, UnorderedPair<T> right) => !left.Equals(right);
}