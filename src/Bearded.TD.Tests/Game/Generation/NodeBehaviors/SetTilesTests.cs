using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public class SetTileTests
    {
        private sealed record TestContext(Tilemap<TileGeometry> SubjectTilemap,
            Tilemap<TileGeometry> ExpectedTilemap, NodeGenerationContext Context)
        {
            public static TestContext CreateForHexagonalNodeWithRadius(int radius)
            {
                var subjectTilemap = new Tilemap<TileGeometry>(radius + 2);
                var expectedTilemap = new Tilemap<TileGeometry>(radius + 2);

                var arbitrarilyPerturbedCenter = Tile.Origin.Neighbor(Direction.UpRight);

                var tiles = Enumerable
                    .Range(0, radius + 1)
                    .SelectMany(r => Tilemap.GetRingCenteredAt(arbitrarilyPerturbedCenter, r));

                var area = Area.From(tiles);

                var context = NodeGenerationContext.Create(
                    subjectTilemap, area,
                    ImmutableArray<Circle>.Empty, ImmutableArray<Tile>.Empty,
                    new LevelGenerationCommandAccumulator(), new Random()
                );

                return new TestContext(subjectTilemap, expectedTilemap, context);
            }

            public void AssertSubjectTilemapEqualsExpectedTilemap()
            {
                SubjectTilemap.Should().HaveSameContentAs(ExpectedTilemap);
            }
        }

        private INodeBehavior<Node> behaviourWithParameters(TileType type)
            => new SetTile(new SetTile.BehaviorParameters(type));

        [Fact]
        public void MakesNoChangesWithEmptyNode()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(-1);

            behaviourWithParameters(TileType.Crevice)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Theory]
        [InlineData(TileType.Floor)]
        [InlineData(TileType.Crevice)]
        [InlineData(TileType.Wall)]
        public void SetsAllTilesInNodeToGivenType(TileType type)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.ExpectedTilemap[tile] = new TileGeometry(type, 0, 0.U());
            }

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Fact]
        public void SetsNoTilesForEmptySelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviourWithParameters(TileType.Crevice)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property]
        public void SetsExactlyThoseTilesInSelection(int seed)
        {
            var random = new Random(seed);
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            foreach (var tile in test.Context.Tiles.All)
            {
                if (random.NextBool())
                    test.Context.Tiles.Selection.Remove(tile);
            }
            foreach (var tile in test.Context.Tiles.Selection)
            {
                test.ExpectedTilemap[tile] = new TileGeometry(TileType.Crevice, 0, 0.U());
            }

            behaviourWithParameters(TileType.Crevice)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }
    }
}
