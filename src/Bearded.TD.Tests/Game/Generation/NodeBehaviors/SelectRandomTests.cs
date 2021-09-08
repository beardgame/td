using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectRandomTests
    {
        private static INodeBehavior<Node> behaviourWithParameters(double percentage)
            => new SelectRandom(new SelectRandom.BehaviourParameters(percentage));

        [Fact]
        public void UnselectsEverythingWithPercentageZeroOrLess()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            behaviourWithParameters(0).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void SelectsAllWithPercentageOneOrGreater()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviourWithParameters(1).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.All.Count);
        }

        [Property]
        public void SelectsApproximatelyGivenPercentage(int seed, byte inverseFraction)
        {
            var fraction = inverseFraction / (double)byte.MaxValue;

            var test = TestContext.CreateForHexagonalNodeWithRadius(10, seed);

            behaviourWithParameters(fraction).Generate(test.Context);

            var expectedSelectedTiles = MoreMath.RoundToInt(test.Context.Tiles.All.Count * fraction);
            var selectedTiles = test.Context.Tiles.Selection.Count;
            selectedTiles.Should().BeInRange(expectedSelectedTiles - 1, expectedSelectedTiles + 1);
        }
    }
}
