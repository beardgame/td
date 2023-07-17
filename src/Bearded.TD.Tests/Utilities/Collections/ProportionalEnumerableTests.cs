using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Utilities.Collections;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Utilities.Collections;

public sealed class ProportionalEnumerableTests
{
    [Fact]
    public void RepeatsSingleElement()
    {
        const string obj = "object";
        var enumerable = new ProportionalEnumerable<object>(new Dictionary<object, int>
        {
            { obj, 3 }
        });

        enumerable.ToImmutableArray().Should().ContainInConsecutiveOrder(obj, obj, obj);
    }

    [Fact]
    public void MixesTwoElements()
    {
        const string obj1 = "object 1";
        const string obj2 = "object 2";
        var enumerable = new ProportionalEnumerable<object>(new Dictionary<object, int>
        {
            { obj1, 3 },
            { obj2, 2 }
        });

        enumerable.ToImmutableArray().Should().ContainInConsecutiveOrder(obj1, obj2, obj1, obj2, obj1);
    }

    [Fact]
    public void AlternatesElementsWithEqualCounts()
    {
        const string obj1 = "object 1";
        const string obj2 = "object 2";
        var enumerable = new ProportionalEnumerable<object>(new Dictionary<object, int>
        {
            { obj1, 2 },
            { obj2, 2 }
        });

        enumerable.ToImmutableArray().Should().ContainInConsecutiveOrder(obj1, obj2, obj1, obj2);
    }
}
