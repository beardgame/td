using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors;

public sealed class AvoidTagAdjacencyTests
{
    public static readonly ImmutableArray<object[]> AllDirections =
        Extensions.Directions.Select(d => new object[] { d }).ToImmutableArray();

    [Fact]
    public void NoAdjacentTagsEvaluatesNoPenalty()
    {
        var ctx = FitnessTestContext.CreateSpiderWithRadius(1);
        var behavior = new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("tag")));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void OneAdjacentTagEvaluatesPenalty(Direction d)
    {
        var ctx = FitnessTestContext.CreateSpiderWithRadius(1);
        ctx.TagNode(Tile.Origin.Neighbor(d), "tag");
        var behavior = new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("tag"), 10));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(10);
    }

    [Fact]
    public void MultipleAdjacentTagsEvaluatesPenaltyMultipleTimes()
    {
        var ctx = FitnessTestContext.CreateSpiderWithRadius(1);
        Extensions.Directions.ForEach(d => ctx.TagNode(Tile.Origin.Neighbor(d), "tag"));
        var behavior = new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("tag"), 10));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(60);
    }

    [Fact]
    public void DisconnectedAdjacencyNotConsideredForPenalty()
    {
        var ctx = FitnessTestContext.CreateSpiderWithRadius(1);
        const Direction d = Direction.Right;
        ctx.TagNode(Tile.Origin.Neighbor(d).Neighbor(d), "tag");
        ctx.Disconnect(Tile.Origin, d);
        var behavior = new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("tag")));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }

    [Fact]
    public void FurtherThanOneStepNotConsideredForPenalty()
    {
        var ctx = FitnessTestContext.CreateSpiderWithRadius(2);
        const Direction d = Direction.Right;
        ctx.TagNode(Tile.Origin.Neighbor(d).Neighbor(d), "tag");
        var behavior = new AvoidTagAdjacency(new AvoidTagAdjacency.BehaviorParameters(new NodeTag("tag")));

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }
}
