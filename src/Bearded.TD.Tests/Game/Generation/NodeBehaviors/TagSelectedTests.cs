using System;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class TagSelectedTests
    {
        private static TagSelected behaviorWithParameters(double value = 0, string? tag = null)
        {
            return new TagSelected(new TagSelected.BehaviorParameters(tag, value));
        }

        [Fact]
        public void BehaviorDoesNotChangeTagsIfValueIsZero()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            behaviorWithParameters().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(0);
            }
        }

        [Fact]
        public void DefaultsToValueOneIfNoneIsGiven()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            new TagSelected(new TagSelected.BehaviorParameters(null)).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(1);
            }
        }

        [Property]
        public void SetsDefaultTagToNonZeroValue(NormalFloat value)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);

            behaviorWithParameters(value.Get).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(value.Get);
            }
        }

        [Property]
        public void AddsParameterValueToExistingTagValue(NormalFloat value, int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags["default"][tile] = random.NextDouble();
            }

            behaviorWithParameters(value.Get).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(value.Get + random.NextDouble());
            }
        }

        [Property]
        public void AddsParameterValueToTagWithGivenName(NonEmptyString tag, NormalFloat value, int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags[tag.Get][tile] = random.NextDouble();
            }

            behaviorWithParameters(value.Get, tag.Get).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags[tag.Get][tile];

                tagValue.Should().Be(value.Get + random.NextDouble());
            }
        }

        [Property]
        public void DoesNotChangeDefaultTagValuesWithOtherName(NonEmptyString tag, NormalFloat value)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var tagName = tag.Get == "default" ? "coincidence!" : tag.Get;

            behaviorWithParameters(value.Get, tagName).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(0);
            }
        }

        [Property]
        public void OnlyChangesTagValueInSelection(NonEmptyString tag, NormalFloat value, int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();
                if (!selected)
                    test.Context.Tiles.Selection.Remove(tile);
                test.Context.Tiles.Tags[tag.Get][tile] = random.NextDouble();
            }

            behaviorWithParameters(value.Get, tag.Get).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags[tag.Get][tile];

                var selected = random.NextBool();
                var randomValue = random.NextDouble();

                if (selected)
                    tagValue.Should().Be(value.Get + randomValue);
                else
                    tagValue.Should().Be(randomValue);
            }
        }
    }
}
