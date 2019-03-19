using Bearded.TD.Game.Generation;
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation
{
    public abstract class TilemapGeneratorTests
    {
        private const int minRadius = 10;

        internal abstract ITilemapGenerator Generator { get; }

        [Property]
        public void GeneratesMapWithCorrectRadius(int r, int seed)
        {
            if (r < 0)
                r = -r;
            if (r < minRadius)
                return;

            var tilemap = Generator.Generate(r, seed);

            tilemap.Radius.Should().Be(r);
        }

        [Property]
        public void GeneratesIdenticalMapsWithSameSeed(int r, int seed)
        {
            if (r < 0)
                r = -r;
            if (r < minRadius)
                return;

            var firstTilemap = Generator.Generate(r, seed);
            var secondTilemap = Generator.Generate(r, seed);

            assertTilemapsEqual(firstTilemap, secondTilemap);
        }

        private static void assertTilemapsEqual(
            Tilemap<TileGeometry.TileType> firstTilemap,
            Tilemap<TileGeometry.TileType> secondTilemap)
        {
            firstTilemap.TilesSpiralOutward.Should().ContainInOrder(secondTilemap.TilesSpiralOutward);
        }
    }
}
