using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation
{
    public abstract class TilemapGeneratorTests
    {
        private static readonly NodeGroup nodes =
            new NodeGroup.Leaf(new TestNodeBlueprint(), new NodeGroup.RandomizedNumber(1, null, null));
        private const int minRadius = 20;

        internal abstract ILevelGenerator Generator { get; }

        /*
        [Property]
        public void GeneratesMapWithCorrectRadius(int r, int seed)
        {
            if (r < 0)
                r = -r;
            if (r < minRadius)
                return;

            var tilemap = Generator.Generate(new LevelGenerationParameters(r, nodes), seed);

            tilemap.Radius.Should().Be(r);
        }

        [Property]
        public void GeneratesIdenticalMapsWithSameSeed(int r, int seed)
        {
            if (r < 0)
                r = -r;
            if (r < minRadius)
                return;

            var firstTilemap = Generator.Generate(new LevelGenerationParameters(r, nodes), seed);
            var secondTilemap = Generator.Generate(new LevelGenerationParameters(r, nodes), seed);

            assertTilemapsEqual(firstTilemap, secondTilemap);
        }

        private static void assertTilemapsEqual(
            Tilemap<TileGeometry> firstTilemap,
            Tilemap<TileGeometry> secondTilemap)
        {
            var firstTilemapGeometries = firstTilemap.TilesSpiralOutward.Select(t => firstTilemap[t]);
            var secondTilemapGeometries = secondTilemap.TilesSpiralOutward.Select(t => secondTilemap[t]);

            firstTilemapGeometries.Should().ContainInOrder(secondTilemapGeometries);
        }
        */
    }
}
