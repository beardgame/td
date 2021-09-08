using System;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectTagTests
    {
        public SelectTagTests()
        {
            Arb.Register<TilemapGenerators>();
        }

        private static INodeBehavior<Node> behaviourWithParameters(
            double threshold = default, string? tag = default, SelectTag.CompareMode mode = default)
            => new SelectTag(new SelectTag.BehaviorParameters(
                threshold, tag, mode));

        [Fact]
        public void ClearsSelectionWithZeroThreshold()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            behaviourWithParameters().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void SelectsEverythingWithNegativeThreshold()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviourWithParameters(-1).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.All.Count);
        }

        [Property]
        public void SelectsTilesWithPositiveDefaultTagValueWithNullTagParameter(int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags["default"][tile] = random.Next(-10, 10);
            }

            behaviourWithParameters().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var shouldBeSelected = test.Context.Tiles.Tags["default"][tile] > 0;
                test.Context.Tiles.Selection.Contains(tile).Should().Be(shouldBeSelected);
            }
        }

        [Property]
        public void SelectsTilesWithPositiveValueForGivenTag(int seed, NonNull<string> tag)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags[tag.Get][tile] = random.Next(-10, 10);
            }

            behaviourWithParameters(tag: tag.Get).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var shouldBeSelected = test.Context.Tiles.Tags[tag.Get][tile] > 0;
                test.Context.Tiles.Selection.Contains(tile).Should().Be(shouldBeSelected);
            }
        }

        [Property]
        public void DoesNotSelectTilesWithPositiveValuesForOtherTags(
            int seed, NonNull<string> behaviourTag, NonNull<string> otherTag)
        {
            var differentTag = behaviourTag.Get != otherTag.Get ? otherTag.Get : otherTag.Get + ".";
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags[differentTag][tile] = random.Next(-10, 10);
            }

            behaviourWithParameters(tag: behaviourTag.Get).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Property]
        public void SelectsTilesGreaterThanThreshold(int seed, NonNull<string> tag, NormalFloat threshold)
        {
            testWithCompareMode(seed, tag, threshold, default, (x, y) => x > y);
        }

        [Property]
        public void SelectsTilesGreaterThanThresholdWithGreaterMode(
            int seed, NonNull<string> tag, NormalFloat threshold)
        {
            testWithCompareMode(seed, tag, threshold, SelectTag.CompareMode.Greater, (x, y) => x > y);
        }

        [Property]
        public void SelectsTilesGreaterOrEqualToThresholdWithGreaterOrEqualMode(
            int seed, NonNull<string> tag, NormalFloat threshold)
        {
            testWithCompareMode(seed, tag, threshold, SelectTag.CompareMode.GreaterOrEqual, (x, y) => x >= y);
        }

        [Property]
        public void SelectsTilesLessThanThresholdWithLessMode(
            int seed, NonNull<string> tag, NormalFloat threshold)
        {
            testWithCompareMode(seed, tag, threshold, SelectTag.CompareMode.Less, (x, y) => x < y);
        }

        [Property]
        public void SelectsTilesLessOrEqualToThresholdWithLessOrEqualMode(
            int seed, NonNull<string> tag, NormalFloat threshold)
        {
            testWithCompareMode(seed, tag, threshold, SelectTag.CompareMode.LessOrEqual, (x, y) => x <= y);
        }

        private void testWithCompareMode(int seed, NonNull<string> tag, NormalFloat threshold,
            SelectTag.CompareMode mode, Func<double, double, bool> expectedComparison)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags[tag.Get][tile] = threshold.Get + random.Next(-10, 10);
            }

            behaviourWithParameters(threshold.Get, tag.Get, mode)
                .Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var shouldBeSelected = expectedComparison(test.Context.Tiles.Tags[tag.Get][tile], threshold.Get);
                test.Context.Tiles.Selection.Contains(tile).Should().Be(shouldBeSelected);
            }
        }
    }
}
