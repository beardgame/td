using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors;

public sealed class ForceDeadEndTests
{
    public static readonly ImmutableArray<object[]> AllDirections =
        Extensions.Directions.Select(d => new object[] { d }).ToImmutableArray();

    [Fact]
    public void NoConnectionsEvaluatesNoPenalty()
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        var behavior = new ForceDeadEnd(new ForceDeadEnd.BehaviorParameters(10));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void OneConnectionEvaluatesNoPenalty(Direction d)
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        ctx.Connect(Tile.Origin, d);
        var behavior = new ForceDeadEnd(new ForceDeadEnd.BehaviorParameters(10));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void MultipleConnectionsEvaluatesPenalty(Direction d)
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        ctx.Connect(Tile.Origin, d);
        ctx.Connect(Tile.Origin, d.Opposite());
        var behavior = new ForceDeadEnd(new ForceDeadEnd.BehaviorParameters(10));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().BeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void MoreConnectionsEvaluatesGreaterPenalty(Direction d)
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        // assuming d = Direction.Right
        //   o   o
        //      / \
        // o   o---o
        //      \
        //   o   o
        ctx.Connect(Tile.Origin, d);
        ctx.Connect(Tile.Origin, d.Next());
        ctx.Connect(Tile.Origin, d.Previous());
        ctx.Connect(Tile.Origin.Neighbor(d), d.Next().Opposite());
        var behavior = new ForceDeadEnd(new ForceDeadEnd.BehaviorParameters(10));

        var threeConnectionPenalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);
        var twoConnectionPenalty = behavior.GetFitnessPenalty(ctx, Tile.Origin.Neighbor(d));

        threeConnectionPenalty.Should().BeGreaterThan(twoConnectionPenalty);
    }
}
