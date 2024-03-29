using System;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class AreaTagValuesTests
    {
        private static AreaTagValues emptyAreaTags() => new(Area.Empty());
        private static AreaTagValues tagsWithTiles(params Tile[] tiles) => new(Area.From(tiles));

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void IfEmptyWontAllowSettingTagValueForAnyTile(Tile tile)
        {
            var tags = emptyAreaTags();

            Action setTile = () => tags[tile] = 1;

            setTile.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void IfEmptyWontAllowGettingTagValueForAnyTile(Tile tile)
        {
            var tags = emptyAreaTags();

            Func<double> getTile = () => tags[tile];

            getTile.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AllowsSettingTagValueForAKnownTile(Tile tile)
        {
            var tags = tagsWithTiles(tile);

            Action setTile = () => tags[tile] = 1;

            setTile.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AllowsGettingTagValueForAKnownTile(Tile tile)
        {
            var tags = tagsWithTiles(tile);

            Func<double> getTile = () => tags[tile];

            getTile.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AllowsSettingTagValueForAnyKnownTile(Tile tile, Tile otherTile)
        {
            var tags = tagsWithTiles(new Tile(1, 2), tile, otherTile);

            Action setTile = () => tags[tile] = 1;

            setTile.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void AllowsGettingTagValueForAnyKnownTile(Tile tile, Tile otherTile)
        {
            var tags = tagsWithTiles(new Tile(1, 2), tile, otherTile);

            Func<double> getTile = () => tags[tile];

            getTile.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void UnsetTagValueIsZero(Tile tile)
        {
            var tags = tagsWithTiles(tile);

            var value = tags[tile];

            value.Should().Be(0);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void ReturnsLastValueSet(Tile tile, double value)
        {
            var tags = tagsWithTiles(tile);

            tags[tile] = 1;
            tags[tile] = 0;
            tags[tile] = -0.5;
            tags[tile] = value;

            tags[tile].Should().Be(value);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void ReturnsDifferentSetValuesForDifferentTiles(Tile tile, Tile otherTile, double value, double otherValue)
        {
            if (tile == otherTile)
                otherTile = otherTile.Neighbor(Direction.Right);

            var tags = tagsWithTiles(tile, otherTile);

            tags[tile] = value;
            tags[otherTile] = otherValue;

            tags[tile].Should().Be(value);
            tags[otherTile].Should().Be(otherValue);
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void ReturnsZeroForATileEvenIfAnotherWasSet(Tile tile, Tile otherTile, double value)
        {
            if (tile == otherTile)
                otherTile = otherTile.Neighbor(Direction.Right);

            var tags = tagsWithTiles(tile, otherTile);

            tags[tile] = value;

            tags[otherTile].Should().Be(0);
        }
    }
}
