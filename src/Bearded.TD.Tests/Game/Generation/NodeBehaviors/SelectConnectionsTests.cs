using System;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectConnectionsTests
    {
        [Fact]
        public void SelectsNothingIfThereAreNoConnections()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2, connectionCount: 0);
            test.Context.Tiles.Selection.RemoveAll();

            new SelectConnections().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Property]
        public void SelectsConnectionTilesOnly(int seed)
        {
            var random = new Random(seed);
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2, seed, 3);
            foreach (var tile in test.Context.Tiles.All)
            {
                if (random.NextBool())
                    test.Context.Tiles.Selection.Remove(tile);
            }

            new SelectConnections().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                test.Context.Tiles.Selection.Contains(tile).Should()
                    .Be(test.Context.NodeData.Connections.Contains(tile));
            }
        }
    }
}
