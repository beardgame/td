using System;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectAllTests
    {
        public SelectAllTests()
        {
            Arb.Register<TilemapGenerators>();
        }

        [Property]
        public void SelectsAll(int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                if (random.NextBool())
                    test.Context.Tiles.Selection.Remove(tile);
            }

            new SelectAll().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.All.Count);
        }
    }
}
