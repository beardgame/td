using System;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class ResetTagSelectedTests
    {
        private static INodeBehavior behaviorWithParameters(string? tag = null, double value = 0)
        {
            return new ResetTagSelected(new ResetTagSelected.BehaviorParameters(tag, value));
        }

        [Property]
        public void SetsDefaultTagValuesToZeroWithoutParameters(int seed)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Tags["default"][tile] = random.NextDouble();
            }

            new ResetTagSelected(new ResetTagSelected.BehaviorParameters(null)).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(0);
            }
        }

        [Property]
        public void OnlySetsTagValuesInSelection(int seed)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();
                if (!selected)
                    test.Context.Tiles.Selection.Remove(tile);
                test.Context.Tiles.Tags["default"][tile] = random.NextDouble();
            }

            behaviorWithParameters().Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                var selected = random.NextBool();
                var randomValue = random.NextDouble();

                if (selected)
                    tagValue.Should().Be(0);
                else
                    tagValue.Should().Be(randomValue);
            }
        }

        [Property]
        public void SetsTagValuesInSelectionToGivenValue(int seed, NormalFloat value)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();
                if (!selected)
                    test.Context.Tiles.Selection.Remove(tile);
                test.Context.Tiles.Tags["default"][tile] = random.NextDouble();
            }

            behaviorWithParameters(value: value.Get).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                var selected = random.NextBool();
                var randomValue = random.NextDouble();

                if (selected)
                    tagValue.Should().Be(value.Get);
                else
                    tagValue.Should().Be(randomValue);
            }
        }

        [Property]
        public void SetsTagValuesForGivenTagName(int seed, NonEmptyString tag, NormalFloat value)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();
                if (!selected)
                    test.Context.Tiles.Selection.Remove(tile);
                test.Context.Tiles.Tags[tag.Get][tile] = random.NextDouble();
            }

            behaviorWithParameters(tag.Get, value.Get).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags[tag.Get][tile];

                var selected = random.NextBool();
                var randomValue = random.NextDouble();

                if (selected)
                    tagValue.Should().Be(value.Get);
                else
                    tagValue.Should().Be(randomValue);
            }
        }

        [Property]
        public void DoesNotChangeDefaultTagValuesWithOtherName(NonEmptyString tag, NormalFloat value)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            var tagName = tag.Get == "default" ? "coincidence!" : tag.Get;

            behaviorWithParameters(tagName, value.Get).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tagValue = test.Context.Tiles.Tags["default"][tile];

                tagValue.Should().Be(0);
            }
        }
    }
}
