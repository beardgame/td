using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors;

public sealed class ForceToCenterTests
{
    public static readonly ImmutableArray<object[]> AllDirections =
        Extensions.Directions.Select(d => new object[] { d }).ToImmutableArray();

    [Fact]
    public void OriginEvaluatesNoPenalty()
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        var behavior = new ForceToCenter();

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin);

        penalty.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void AwayFromCenterEvaluatesPenalty(Direction d)
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(1);
        var behavior = new ForceToCenter();

        var penalty = behavior.GetFitnessPenalty(ctx, Tile.Origin.Neighbor(d));

        penalty.Should().BeGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(AllDirections))]
    public void FurtherFromCenterEvaluatesGreaterPenalty(Direction d)
    {
        var ctx = FitnessTestContext.UnconnectedWithRadius(2);
        var behavior = new ForceToCenter();

        var closerPenalty = behavior.GetFitnessPenalty(ctx, Tile.Origin.Neighbor(d));
        var furtherPenalty = behavior.GetFitnessPenalty(ctx, Tile.Origin.Neighbor(d).Neighbor(d));

        furtherPenalty.Should().BeGreaterThan(closerPenalty);
    }
}
