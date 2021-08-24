using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;
using static System.Array;
using static System.Math;
using static Bearded.TD.Tiles.Direction;
using Direction = Bearded.TD.Tiles.Direction;

namespace Bearded.TD.Tests.Tiles
{
    public sealed class TileTests
    {
        [Property]
        public void ConstructsWithCoordinates(int x, int y)
        {
            var tile = new Tile(x, y);

            tile.X.Should().Be(x);
            tile.Y.Should().Be(y);
        }

        [Property]
        public void EqualsOtherWithSameCoordinates(int x, int y)
        {
            var tile = new Tile(x, y);
            var tile2 = new Tile(x, y);

            tile.Equals(tile2).Should().BeTrue();
            tile.Equals((object)tile2).Should().BeTrue();
            (tile == tile2).Should().BeTrue();
            (tile != tile2).Should().BeFalse();
        }

        [Theory]
        [InlineData(0, 0, 0, 1)]
        [InlineData(1, 0, 0, 1)]
        [InlineData(0, 0, 0, -1)]
        [InlineData(0, -1, 0, 1)]
        [InlineData(0, 1, 1, 0)]
        public void DoesNotEqualOtherWithDifferentCoordinates(int x, int y, int x2, int y2)
        {
            var tile = new Tile(x, y);
            var tile2 = new Tile(x2, y2);

            tile.Equals(tile2).Should().BeFalse();
            tile.Equals((object)tile2).Should().BeFalse();
            (tile == tile2).Should().BeFalse();
            (tile != tile2).Should().BeTrue();
        }

        [Property]
        public void DoesNotEqualNull(int x, int y)
        {
            var tile = new Tile(x, y);

            tile.Equals(null).Should().BeFalse();
        }

        [Property]
        public void DoesNotEqualOtherType(int x, int y)
        {
            var tile = new Tile(x, y);

            // ReSharper disable once SuspiciousTypeConversion.Global
            tile.Equals("").Should().BeFalse();
            // ReSharper disable once SuspiciousTypeConversion.Global
            tile.Equals(tile.ToString()).Should().BeFalse();
            tile.Equals(new object()).Should().BeFalse();
        }

        [Fact]
        public void OriginIsZeroZero()
        {
            var origin = Tile.Origin;

            origin.Should().Be(new Tile(0, 0));
        }

        [Fact]
        public void DefaultValueIsOrigin()
        {
            var defaultTile = default(Tile);

            defaultTile.Should().Be(Tile.Origin);
        }

        [Fact]
        public void RadiusAtOriginIsZero()
        {
            var tile = Tile.Origin;

            tile.Radius.Should().Be(0);
        }

        [Property]
        public void RadiusAtYEqualsZeroIsAbsoluteX(int x)
        {
            var tile = new Tile(x, 0);

            tile.Radius.Should().Be(Abs(x));
        }

        [Property]
        public void RadiusAtXEqualsZeroIsAbsoluteY(int y)
        {
            var tile = new Tile(0, y);

            tile.Radius.Should().Be(Abs(y));
        }

        [Fact]
        public void CanReachSameTileWithDifferentSeriesOfSteps()
        {
            var tile = Tile.Origin.Neighbor(Left).Neighbor(UpLeft);
            var tile2 = Tile.Origin.Neighbor(DownLeft).Neighbor(Left).Neighbor(UpLeft).Neighbor(UpRight);

            tile2.Should().Be(tile);
        }

        [Theory]
        [MemberData(nameof(TotalDistanceAndStepLists))]
        public void RadiusIsCorrectAfterKnownSteps(int expectedRadius, Direction[] steps)
        {
            var tile = Tile.Origin;
            foreach (var direction in steps)
            {
                tile = tile.Neighbor(direction);
            }

            tile.Radius.Should().Be(expectedRadius);
        }

        [Property]
        public void DistanceToSelfIsZero(int x, int y)
        {
            var tile = new Tile(x, y);

            tile.DistanceTo(tile).Should().Be(0);
        }

        [Theory]
        [MemberData(nameof(ArbitraryTilesJoinedWithTotalDistancesAndStepLists))]
        public void DistanceIsCorrectAfterKnownSteps(Tile startingTile, int expectedDistance, Direction[] steps)
        {
            var tile = startingTile;
            foreach (var direction in steps)
            {
                tile = tile.Neighbor(direction);
            }

            tile.DistanceTo(startingTile).Should().Be(expectedDistance);
            startingTile.DistanceTo(tile).Should().Be(expectedDistance);
        }

        [Property]
        public void SubtractingTilesIsReversibleWithStepAddition(int x, int y, int x2, int y2)
        {
            var tile = new Tile(x, y);
            var tile2 = new Tile(x2, y2);

            var step = tile2 - tile;

            var tileWithDifference = tile + step;

            tileWithDifference.Should().Be(tile2);
        }

        [Property]
        public void DeconstructsIntoComponents(int x, int y)
        {
            var tile = new Tile(x, y);

            var (component1, component2) = tile;

            component1.Should().Be(x);
            component2.Should().Be(y);
        }

        [Property]
        public void HashCodeOfEqualTilesEqual(int x, int y)
        {
            var tile = new Tile(x, y);
            var tile2 = new Tile(x, y);

            tile.GetHashCode().Should().Be(tile2.GetHashCode());
        }

        public static IEnumerable<object[]> TotalDistanceAndStepLists { get; } =
            new List<object[]>
            {
                new object[] { 0, Empty<Direction>() },
                new object[] { 1, new [] { Right } },
                new object[] { 1, new [] { UpRight } },
                new object[] { 1, new [] { UpLeft } },
                new object[] { 1, new [] { Left } },
                new object[] { 1, new [] { DownLeft } },
                new object[] { 1, new [] { DownRight } },
                new object[] { 1, new [] { Right, UpLeft } },
                new object[] { 2, new [] { Right, UpRight } },
                new object[] { 2, new [] { Right, UpLeft, UpLeft } },
                new object[] { 2, new [] { DownRight, Left, DownRight } },
                new object[] { 2, new [] { UpRight, Left, UpLeft, DownLeft } },
                new object[] { 3, new [] { Left, DownLeft, Left } },
                new object[] { 3, new [] { DownLeft, DownLeft, DownRight, Right, Right } },
            };

        public static IEnumerable<object[]> ArbitraryTilesJoinedWithTotalDistancesAndStepLists { get; } =
            (from tile in TileTestConstants.SomeArbitraryTiles
                from otherParameters in TotalDistanceAndStepLists
                select otherParameters.Prepend(tile).ToArray()).ToList();
    }
}
