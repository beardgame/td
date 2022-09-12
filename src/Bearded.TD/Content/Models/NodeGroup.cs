using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Content.Models;

abstract record NodeGroup
{
    public NumberRestriction Number { get; }

    private NodeGroup(NumberRestriction number)
    {
        Number = number;
    }

    public sealed record Leaf(INodeBlueprint Blueprint, NumberRestriction Number) : NodeGroup(Number);

    public sealed record Composite(ImmutableArray<NodeGroup> Children, NumberRestriction Number)
        : NodeGroup(Number);

    public abstract record NumberRestriction;

    public sealed record FixedNumber(int Number) : NumberRestriction;

    public sealed record RandomizedNumber(double Weight, int? MinNumber, int? MaxNumber) : NumberRestriction;
}
