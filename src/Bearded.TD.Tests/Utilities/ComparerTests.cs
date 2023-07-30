using System;
using System.Collections.Generic;
using Bearded.TD.Utilities;
using FluentAssertions;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Utilities;

public sealed class ComparerTests
{
    [Property]
    public void ComparesByChosenSelector(int i1, int i2)
    {
        var min = Math.Min(i1, i2);
        var max = Math.Max(i1, i2);

        var list = new List<SingleInt> { new(i1), new(i2) };
        var comparer = Comparer.Comparing((SingleInt si) => si.Value);

        list.Sort(comparer);

        list[0].Should().Be(new SingleInt(min));
        list[1].Should().Be(new SingleInt(max));
    }

    [Property]
    public void ComparesInReverse(int i1, int i2)
    {
        var min = Math.Min(i1, i2);
        var max = Math.Max(i1, i2);

        var list = new List<SingleInt> { new(i1), new(i2) };
        var comparer = Comparer.Comparing((SingleInt si) => si.Value).Reversed();

        list.Sort(comparer);

        list[0].Should().Be(new SingleInt(max));
        list[1].Should().Be(new SingleInt(min));
    }

    [Property]
    public void CanCompareIfFirstValueIsEqual(int i, int j1, int j2)
    {
        var min = Math.Min(j1, j2);
        var max = Math.Max(j1, j2);

        var list = new List<TwoInts> { new(i, j1), new(i, j2) };
        var comparer = Comparer.Comparing((TwoInts ti) => ti.Value1).ThenComparing(ti => ti.Value2);

        list.Sort(comparer);

        list[0].Should().Be(new TwoInts(i, min));
        list[1].Should().Be(new TwoInts(i, max));
    }

    [Property]
    public void EarlierComparerTakesPrecedence(int i1, int i2, int j1, int j2)
    {
        var min = Math.Min(i1, i2);
        var max = Math.Max(i1, i2);

        var list = new List<TwoInts> { new(i1, j1), new(i2, j2) };
        var comparer = Comparer.Comparing((TwoInts ti) => ti.Value1).ThenComparing(ti => ti.Value2);

        list.Sort(comparer);

        list[0].Value1.Should().Be(min);
        list[1].Value1.Should().Be(max);
    }

    private readonly record struct SingleInt(int Value);

    private readonly record struct TwoInts(int Value1, int Value2);
}
