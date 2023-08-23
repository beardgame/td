using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectRandomSubsetTests
    {
        private static INodeBehavior behaviorWithParameters(int numTiles)
            => new SelectRandomSubset(new SelectRandomSubset.BehaviorParameters(numTiles, null));

        [Fact]
        public void UnselectsEverythingWithNumTilesZero()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);

            behaviorWithParameters(0).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void SelectsEverythingWithNumTilesLargerThanArea()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);

            behaviorWithParameters(Tilemap.TileCountForRadius(2) + 1).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.Selection.Count);
        }

        [Fact]
        public void SelectsTilesWithinExistingSelectionOnly()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(3);
            erodeSelection(test.Context);
            var originalSelection = test.Context.Tiles.Selection.ToImmutableHashSet();

            behaviorWithParameters(Tilemap.TileCountForRadius(3)).Generate(test.Context);

            var newSelection = test.Context.Tiles.Selection.ToImmutableHashSet();
            newSelection.Should().Equal(originalSelection);
        }

        [Fact]
        public void SelectsExactlyNumTiles()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);

            behaviorWithParameters(2).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(2);
        }

        private static void erodeSelection(NodeGenerationContext context)
        {
            var eroded = context.Tiles.Selection.Erode();
            context.Tiles.Selection.RemoveAll();
            foreach (var tile in eroded)
            {
                context.Tiles.Selection.Add(tile);
            }
        }
    }
}
