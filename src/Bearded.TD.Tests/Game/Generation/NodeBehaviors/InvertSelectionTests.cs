using System;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class InvertSelectionTests
    {
        [Fact]
        public void ClearsSelectionIfEverythingIsSelected()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            new InvertSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void SelectsEverythingIfNothingIsSelected()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            new InvertSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.All.Count);
        }

        [Property]
        public void ExactlyInvertsEveryTilesSelection(int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var shouldBeSelected = random.NextBool();
                if (!shouldBeSelected)
                    test.Context.Tiles.Selection.Remove(tile);
            }

            new InvertSelection().Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var shouldBeSelected = !random.NextBool();
                test.Context.Tiles.Selection.Contains(tile).Should().Be(shouldBeSelected);
            }
        }
    }
}
