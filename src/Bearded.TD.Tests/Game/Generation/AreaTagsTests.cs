using System;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation
{
    public sealed class AreaTagsTests
    {
        private static AreaTags tagsWithTiles(params Tile[] tiles) => new (Area.From(tiles));

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void CanGetLayerWithAnyName(NonNull<string> name)
        {
            var tags = tagsWithTiles();

            var layer = tags[name.Get];

            layer.Should().NotBeNull();
        }

        [Fact]
        public void DoesNotAllowGettingLayerWithNullName()
        {
            var tags = tagsWithTiles();

            Func<AreaTagValues> getLayerWithNullName = () => tags[null!];

            getLayerWithNullName.Should().Throw<ArgumentException>();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void LayersIncludeTilesInGivenArea(NonNull<string> name, Tile tile, Tile otherTile)
        {
            var tags = tagsWithTiles(tile, otherTile);

            var layer = tags[name.Get];

            Action settingTilesInKnownTiles = () =>
            {
                layer[tile] = 1;
                layer[otherTile] = 2;
            };

            settingTilesInKnownTiles.Should().NotThrow();
        }

        [Property(Arbitrary = new[] { typeof(TileGenerator) })]
        public void LayersExcludeSettingTilesInGivenArea(NonNull<string> name, Tile tile, Tile otherTile)
        {
            var tags = tagsWithTiles(tile);

            if (tile == otherTile)
                otherTile = otherTile.Neighbor(Direction.Right);

            var layer = tags[name.Get];

            Action settingTilesInKnownTiles = () =>
            {
                layer[otherTile] = 1;
            };

            settingTilesInKnownTiles.Should().Throw<ArgumentException>();
        }

        [Property]
        public void ReturnsSameLayerForSameName(NonNull<string> name)
        {
            var tags = tagsWithTiles();

            var layer = tags[name.Get];
            var layer2 = tags[name.Get];

            layer2.Should().Be(layer);
        }

        [Property]
        public void ReturnsDifferentLayersForDifferentNames(NonNull<string> name, NonNull<string> otherName)
        {
            var tags = tagsWithTiles();

            var nameString = otherName.Get;
            var otherNameString = otherName.Get;
            if (otherNameString == nameString)
                otherNameString += " ";

            var layer = tags[nameString];
            var layer2 = tags[otherNameString];

            layer2.Should().NotBe(layer);
        }
    }
}
