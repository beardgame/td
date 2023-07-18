using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class BoundedMutableAreaTests
    {
        private static BoundedMutableArea areaWithTiles(params Tile[] tiles) => new(Area.From(tiles));
        private static BoundedMutableArea areaWithTiles(IEnumerable<Tile> tiles) => new(Area.From(tiles));

        [Fact]
        public void NewEmptyAreaIsEmpty()
        {
            var area = areaWithTiles();

            area.Count.Should().Be(0);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void NewEmptyAreaContainsNoTiles(Tile tile)
        {
            var area = areaWithTiles();

            area.Contains(tile).Should().BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void NewAreaHasSameSizeAsGivenArea(ushort count)
        {
            var tiles = Enumerable.Range(0, count).Select(c => new Tile(c, c)).ToList();
            var area = areaWithTiles(tiles);

            area.Count.Should().Be(tiles.Count);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void NewAreaContainsAllTilesInOriginalArea(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.Contains(tile).Should().BeTrue();
            area.Contains(otherTile).Should().BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AfterRemoveAllIsEmpty(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.RemoveAll();

            area.Count.Should().Be(0);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AfterRemoveAllContainsNoTiles(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.RemoveAll();

            area.Contains(tile).Should().BeFalse();
            area.Contains(otherTile).Should().BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AfterRemoveAllAndResetHasOriginalCount(ushort count)
        {
            var tiles = Enumerable.Range(0, count).Select(c => new Tile(c, c)).ToList();
            var area = areaWithTiles(tiles);

            area.RemoveAll();
            area.Reset();

            area.Count.Should().Be(tiles.Count);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AfterRemoveAllAndResetHasOriginalTiles(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.RemoveAll();
            area.Reset();

            area.Contains(tile).Should().BeTrue();
            area.Contains(otherTile).Should().BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void RemovingTileLowersCount(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);
            var originalCount = area.Count;

            area.Remove(tile);

            area.Count.Should().Be(originalCount - 1);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void DoesNotContainRemovedTile(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.Remove(tile);

            area.Contains(tile).Should().BeFalse();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void RemovingTileOutsideBoundsDoesNothing(Tile tile)
        {
            var area = areaWithTiles(tile);

            area.Remove(tile.Neighbor(Direction.Right));

            area.Count.Should().Be(1);
            area.Contains(tile).Should().BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AddingTileToEmptyAreaFails(Tile tile)
        {
            var area = areaWithTiles();

            Action addingTileToEmpty = () => area.Add(tile);

            addingTileToEmpty.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AddingTileOutOfBoundsFails(Tile tile, Tile otherTile)
        {
            if (tile == otherTile)
                otherTile = otherTile.Neighbor(Direction.Right);

            var area = areaWithTiles(tile);

            Action addingTileOutOfBounds = () => area.Add(otherTile);

            addingTileOutOfBounds.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AddingTileInBoundsSucceeds(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            Action addingTilesInBounds = () =>
            {
                area.Add(tile);
                area.Add(otherTile);
            };

            addingTilesInBounds.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AddingTilesAlreadyContainedDoesNotIncreaseCount(Tile tile)
        {
            var area = areaWithTiles(tile);

            area.Add(tile);

            area.Count.Should().Be(1);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AddingRemovedTilesIncreasesCount(Tile tile, Tile otherTile)
        {
            if (tile == otherTile)
                otherTile = otherTile.Neighbor(Direction.Right);

            var area = areaWithTiles(tile, otherTile);

            area.RemoveAll();
            area.Add(tile);
            area.Count.Should().Be(1);
            area.Add(otherTile);
            area.Count.Should().Be(2);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void ContainsTilesAddedAfterRemovingThem(Tile tile, Tile otherTile)
        {
            var area = areaWithTiles(tile, otherTile);

            area.RemoveAll();
            area.Add(tile);
            area.Contains(tile).Should().BeTrue();
            area.Add(otherTile);
            area.Contains(otherTile).Should().BeTrue();
        }

        [Fact]
        public void CorrectlyImplementsIAreaEnumerations()
        {
            var tiles = Enumerable.Range(0, 100).Select(c => new Tile(c, c)).ToList();
            var area = areaWithTiles(tiles);

            area.Remove(tiles[0]);
            var expectedTiles = tiles.Skip(1).ToList();

            var enumerated = new List<Tile>();
            foreach (var t in area)
            {
                enumerated.Add(t);
            }

            enumerated.Should().BeEquivalentTo(expectedTiles);

            var asIArea = (IArea)area;

            asIArea.Should().BeEquivalentTo(expectedTiles);
        }
    }
}
