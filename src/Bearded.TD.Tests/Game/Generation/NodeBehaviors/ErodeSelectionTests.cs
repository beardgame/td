using System.Linq;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class ErodeSelectionTests
    {
        [Fact]
        public void DoesNotModifyEmptySelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            new ErodeSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void RemovesAllTilesFromLineSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(0, 0));
            test.Context.Tiles.Selection.Add(new Tile(1, 0));
            test.Context.Tiles.Selection.Add(new Tile(2, 0));

            new ErodeSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }
        [Fact]
        public void RemovesOuterRingFromFullSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);

            new ErodeSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileSelected = test.Context.Tiles.Selection.Contains(tile);
                var tileIsNotOnNodeEdge = tile.PossibleNeighbours().All(test.Context.Tiles.All.Contains);

                tileSelected.Should().Be(tileIsNotOnNodeEdge);
            }
        }

        [Fact]
        public void RemovesOuterRingFromHexagonalSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in Tilemap.GetSpiralCenteredAt(Tile.Origin, 3))
            {
                test.Context.Tiles.Selection.Add(tile);
            }

            new ErodeSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(tile.Radius < 3);
            }
        }

        [Fact]
        public void ThinsVeryThickSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in Tilemap.GetSpiralCenteredAt(Tile.Origin, 4))
            {
                var inThickLine = tile.X is >= -1 and <= 1;

                if (inThickLine)
                    test.Context.Tiles.Selection.Add(tile);
            }

            new ErodeSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var inThinLine = tile.X == 0;

                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(inThinLine && tile.Radius < 4);
            }
        }

        [Fact]
        public void ExpandsHolesInSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.Remove(new Tile(1, 1));

            new ErodeSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileIsInExpandedHole = tile.DistanceTo(new Tile(1, 1)) <= 1;
                var tileIsNotOnNodeEdge = tile.PossibleNeighbours().All(test.Context.Tiles.All.Contains);

                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(tileIsNotOnNodeEdge && !tileIsInExpandedHole);
            }
        }
    }
}
