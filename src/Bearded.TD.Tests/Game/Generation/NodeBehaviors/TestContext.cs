using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    sealed record TestContext(Tilemap<TileGeometry> SubjectTilemap,
        Tilemap<TileGeometry> ExpectedTilemap, NodeGenerationContext Context)
    {
        public static TestContext CreateEmpty() => CreateForHexagonalNodeWithRadius(-1);
        public static TestContext CreateForHexagonalNodeWithRadius(int radius, int randomSeed = 0)
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
                new LevelGenerationCommandAccumulator(), new Random(randomSeed)
            );

            return new TestContext(subjectTilemap, expectedTilemap, context);
        }

        public void AssertSubjectTilemapEqualsExpectedTilemap()
        {
            SubjectTilemap.Should().HaveSameContentAs(ExpectedTilemap);
        }
    }
}
