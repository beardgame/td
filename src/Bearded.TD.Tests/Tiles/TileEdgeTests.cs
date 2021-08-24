using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Tiles;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Tiles
{
    public sealed class TileEdgeTests
    {
        [Property]
        public void ConstructsEdgeAdjacentToTile(int x, int y, Direction d)
        {
            if (d == Direction.Unknown) return;

            var tile = new Tile(x, y);

            var tileEdge = TileEdge.From(tile, d);

            var (tile1, tile2) = tileEdge.AdjacentTiles;
            new[] { tile1, tile2 }.Should().Contain(tile);
        }

        [Property]
        public void ConstructsEdgeAdjacentToTileInDirectionStep(int x, int y, Direction d)
        {
            if (d == Direction.Unknown) return;

            var tile = new Tile(x, y);

            var tileEdge = TileEdge.From(tile, d);

            var (tile1, tile2) = tileEdge.AdjacentTiles;
            new[] { tile1, tile2 }.Should().Contain(tile.Neighbor(d));
        }

        [Property]
        public void ConstructsValidInstanceWhenValidDirection(int x, int y, Direction d)
        {
            if (d == Direction.Unknown) return;

            var tile = new Tile(x, y);

            var tileEdge = TileEdge.From(tile, d);

            tileEdge.IsValid.Should().BeTrue();
        }

        [Property]
        public void ConstructsValidInstanceWhenUnknownDirection(int x, int y)
        {
            var tile = new Tile(x, y);

            var tileEdge = TileEdge.From(tile, Direction.Unknown);

            tileEdge.IsValid.Should().BeFalse();
        }

        [Fact]
        public void DefaultIsInvalid()
        {
            var tileEdge = new TileEdge();

            tileEdge.IsValid.Should().BeFalse();
        }

        [Property]
        public void AdjacentTilesThrowsForInvalidTileEdge(int x, int y)
        {
            var tile = new Tile(x, y);
            var tileEdge = TileEdge.From(tile, Direction.Unknown);

            Func<(Tile, Tile)> action = () => tileEdge.AdjacentTiles;

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Property]
        public void EqualsOtherWithSameTileAndDirection(int x, int y, Direction d)
        {
            var tile = new Tile(x, y);

            var tileEdge1 = TileEdge.From(tile, d);
            var tileEdge2 = TileEdge.From(tile, d);

            tileEdge1.Equals(tileEdge2).Should().BeTrue();
            tileEdge1.Equals((object)tileEdge2).Should().BeTrue();
            (tileEdge1 == tileEdge2).Should().BeTrue();
            (tileEdge1 != tileEdge2).Should().BeFalse();
            tileEdge1.GetHashCode().Should().Be(tileEdge2.GetHashCode());
        }

        [Property]
        public void EqualsEdgeConstructedFromOppositeTile(int x, int y, Direction d)
        {
            if (d == Direction.Unknown) return;

            var tile1 = new Tile(x, y);
            var tile2 = tile1.Neighbor(d);

            var tileEdge1 = TileEdge.From(tile1, d);
            var tileEdge2 = TileEdge.From(tile2, d.Opposite());

            tileEdge1.Equals(tileEdge2).Should().BeTrue();
            tileEdge1.Equals((object)tileEdge2).Should().BeTrue();
            (tileEdge1 == tileEdge2).Should().BeTrue();
            (tileEdge1 != tileEdge2).Should().BeFalse();
            tileEdge1.GetHashCode().Should().Be(tileEdge2.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(ArbitraryTilesJoinedWithAllDirectionPairs))]
        public void DoesNotEqualEdgeWithDifferentDirection(Tile t, Direction d1, Direction d2)
        {
            var tileEdge1 = TileEdge.From(t, d1);
            var tileEdge2 = TileEdge.From(t, d2);

            tileEdge1.Equals(tileEdge2).Should().BeFalse();
            tileEdge1.Equals((object)tileEdge2).Should().BeFalse();
            (tileEdge1 == tileEdge2).Should().BeFalse();
            (tileEdge1 != tileEdge2).Should().BeTrue();
        }

        [Theory]
        [InlineData(0, 0, 0, 1, Direction.Right)]
        [InlineData(1, 0, 0, 1, Direction.UpRight)]
        [InlineData(0, 0, 0, -1, Direction.UpLeft)]
        [InlineData(0, -1, 0, 1, Direction.Left)]
        [InlineData(0, 1, 1, 0, Direction.DownLeft)]
        [InlineData(0, 1, 1, 1, Direction.DownRight)]
        [InlineData(0, 1, 1, -1, Direction.UpRight)]
        public void DoesNotEqualEdgeWithDifferentTile(int x1, int y1, int x2, int y2, Direction d)
        {
            var tile1 = new Tile(x1, y1);
            var tile2 = new Tile(x2, y2);

            var tileEdge1 = TileEdge.From(tile1, d);
            var tileEdge2 = TileEdge.From(tile2, d);

            tileEdge1.Equals(tileEdge2).Should().BeFalse();
            tileEdge1.Equals((object)tileEdge2).Should().BeFalse();
            (tileEdge1 == tileEdge2).Should().BeFalse();
            (tileEdge1 != tileEdge2).Should().BeTrue();
        }

        [Property]
        public void DoesNotEqualNull(int x, int y, Direction d)
        {
            var tile = new Tile(x, y);
            var tileEdge = TileEdge.From(tile, d);

            tileEdge.Equals(null).Should().BeFalse();
        }

        [Property]
        public void DoesNotEqualOtherType(int x, int y, Direction d)
        {
            var tile = new Tile(x, y);
            var tileEdge = TileEdge.From(tile, d);

            // ReSharper disable once SuspiciousTypeConversion.Global
            tileEdge.Equals("").Should().BeFalse();
            // ReSharper disable once SuspiciousTypeConversion.Global
            tileEdge.Equals(tile.ToString()).Should().BeFalse();
            tileEdge.Equals(new object()).Should().BeFalse();
        }

        [Property]
        public void CanWriteAndReadDataFromTilemap(int x, int y, Direction d, int r, string data)
        {
            if (d == Direction.Unknown) return;

            r = Math.Abs(r);
            if (r < 5) return;

            var tile = new Tile(x, y);
            var tilemap = new Tilemap<ModifiableEdges>(Math.Max(tile.Radius + 1, r), _ => ModifiableEdges.Empty);

            var edge = TileEdge.From(tile, d);

            edge.ModifyEdgeIn(tilemap, data);
            edge.GetEdgeFrom<ModifiableEdges, string>(tilemap).Should().Be(data);
        }

        [Property]
        public void CannotWriteWithInvalidEdge(int x, int y, int r, string data)
        {
            r = Math.Abs(r);
            if (r < 5) return;

            var tile = new Tile(x, y);
            var tilemap = new Tilemap<ModifiableEdges>(Math.Max(tile.Radius + 1, r), _ => ModifiableEdges.Empty);

            var edge = TileEdge.From(tile, Direction.Unknown);

            Action action = () => edge.ModifyEdgeIn(tilemap, data);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Property]
        public void CannotRedWithInvalidEdge(int x, int y, int r, string data)
        {
            r = Math.Abs(r);
            if (r < 5) return;

            var tile = new Tile(x, y);
            var tilemap = new Tilemap<ModifiableEdges>(Math.Max(tile.Radius + 1, r), _ => ModifiableEdges.Empty);

            var edge = TileEdge.From(tile, Direction.Unknown);

            Func<string> action = () => edge.GetEdgeFrom<ModifiableEdges, string>(tilemap);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        private static IEnumerable<object[]> allDirectionPairs { get; } =
            (from d1 in Extensions.Directions
                from d2 in Extensions.Directions
                where d2 > d1
                select new object[] { d1, d2 }).ToImmutableArray();

        public static IEnumerable<object[]> ArbitraryTilesJoinedWithAllDirectionPairs { get; } =
            (from tile in TileTestConstants.SomeArbitraryTiles
                from directions in allDirectionPairs
                select directions.Prepend(tile).ToArray()).ToImmutableArray();

        private record ModifiableEdges(string Right, string UpRight, string UpLeft)
            : IModifiableTileEdges<ModifiableEdges, string>
        {
            public ModifiableEdges WithRight(string data) => this with { Right = data };
            public ModifiableEdges WithUpRight(string data) => this with { UpRight = data };
            public ModifiableEdges WithUpLeft(string data) => this with { UpLeft = data };

            public static ModifiableEdges Empty { get; } = new("", "", "");
        }
    }
}
