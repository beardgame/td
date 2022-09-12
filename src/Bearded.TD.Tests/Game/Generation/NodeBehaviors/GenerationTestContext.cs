using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    sealed record GenerationTestContext(Tilemap<TileGeometry> SubjectTilemap,
        Tilemap<TileGeometry> ExpectedTilemap, NodeGenerationContext Context)
    {
        public static GenerationTestContext CreateEmpty() => CreateForHexagonalNodeWithRadius(-1);
        public static GenerationTestContext CreateForHexagonalNodeWithRadius(int radius, int randomSeed = 0, int connectionCount = 0)
        {
            var random = new Random(randomSeed);

            var subjectTilemap = new Tilemap<TileGeometry>(radius + 2);
            var expectedTilemap = new Tilemap<TileGeometry>(radius + 2);

            var arbitrarilyPerturbedCenter = Tile.Origin.Neighbor(Direction.UpRight);

            var tiles = Enumerable
                .Range(0, radius + 1)
                .SelectMany(r => Tilemap.GetRingCenteredAt(arbitrarilyPerturbedCenter, r));

            var area = Area.From(tiles);

            var connections = radius <= 0 && connectionCount > 0
                ? ImmutableArray<Tile>.Empty
                : Tilemap.GetRingCenteredAt(arbitrarilyPerturbedCenter, radius)
                    .RandomSubset(connectionCount, random).ToImmutableArray();

            var context = NodeGenerationContext.Create(
                subjectTilemap, area,
                ImmutableArray<Circle>.Empty, connections,
                new LevelGenerationCommandAccumulator(), random
            );

            return new GenerationTestContext(subjectTilemap, expectedTilemap, context);
        }

        public void AssertSubjectTilemapEqualsExpectedTilemap()
        {
            SubjectTilemap.Should().HaveSameContentAs(ExpectedTilemap);
        }
    }
}
