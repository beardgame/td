using System;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class ErodeSelectionTests
    {
        private static ErodeSelection behaviorWithDefaultParameters()
        {
            return new ErodeSelection(new ErodeSelection.BehaviorParameters());
        }

        private static ErodeSelection behaviorWithParameters(int strength)
        {
            return new ErodeSelection(new ErodeSelection.BehaviorParameters(strength));
        }

        [Fact]
        public void DoesNotModifyEmptySelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviorWithDefaultParameters().Generate(test.Context);

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

            behaviorWithDefaultParameters().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void RemovesOuterRingFromFullSelection()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);

            behaviorWithDefaultParameters().Generate(test.Context);

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

            behaviorWithDefaultParameters().Generate(test.Context);

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

            behaviorWithDefaultParameters().Generate(test.Context);

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

            behaviorWithDefaultParameters().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileIsInExpandedHole = tile.DistanceTo(new Tile(1, 1)) <= 1;
                var tileIsNotOnNodeEdge = tile.PossibleNeighbours().All(test.Context.Tiles.All.Contains);

                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(tileIsNotOnNodeEdge && !tileIsInExpandedHole);
            }
        }

        [Property]
        public void DoesNotChangeSelectionWithStrengthZero(int seed)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            var random = new Random(seed);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();
                if (selected)
                    test.Context.Tiles.Selection.Add(tile);
            }

            behaviorWithParameters(0).Generate(test.Context);

            random = new Random(seed);
            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = random.NextBool();

                test.Context.Tiles.Selection.Contains(tile).Should().Be(selected);
            }
        }

        [Fact]
        public void RemovesOnlySingleTilesWithStrengthOne()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(0, 0));
            test.Context.Tiles.Selection.Add(new Tile(1, 0));
            test.Context.Tiles.Selection.Add(new Tile(2, 0));
            test.Context.Tiles.Selection.Add(new Tile(4, 0));

            behaviorWithParameters(1).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var selected =
                    tile == new Tile(0, 0) ||
                    tile == new Tile(1, 0) ||
                    tile == new Tile(2, 0);

                test.Context.Tiles.Selection.Contains(tile).Should().Be(selected);
            }
        }

        [Fact]
        public void RemovesEndTilesFromLineSelectionWithStrengthTwo()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(3);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(0, 0));
            test.Context.Tiles.Selection.Add(new Tile(1, 0));
            test.Context.Tiles.Selection.Add(new Tile(2, 0));
            test.Context.Tiles.Selection.Add(new Tile(3, 0));

            behaviorWithParameters(2).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var selected =
                    tile == new Tile(1, 0) ||
                    tile == new Tile(2, 0);

                test.Context.Tiles.Selection.Contains(tile).Should().Be(selected);
            }
        }

        [Fact]
        public void DoesNotModifyHexagonSelectionWithStrengthTwo()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in Tilemap.GetSpiralCenteredAt(Tile.Origin, 3))
            {
                test.Context.Tiles.Selection.Add(tile);
            }

            behaviorWithParameters(2).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(tile.Radius <= 3);
            }
        }

        [Fact]
        public void RemovesAllTilesFromLineSelectionWithStrengthThree()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(3);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(0, 0));
            test.Context.Tiles.Selection.Add(new Tile(1, 0));
            test.Context.Tiles.Selection.Add(new Tile(2, 0));
            test.Context.Tiles.Selection.Add(new Tile(3, 0));

            behaviorWithParameters(3).Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void DoesNotModifyHexagonSelectionWithStrengthThree()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in Tilemap.GetSpiralCenteredAt(Tile.Origin, 3))
            {
                test.Context.Tiles.Selection.Add(tile);
            }

            behaviorWithParameters(3).Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var tileSelected = test.Context.Tiles.Selection.Contains(tile);

                tileSelected.Should().Be(tile.Radius <= 3);
            }
        }

        [Fact]
        public void RemovesCornersFromHexagonSelectionWithStrengthFour()
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(5);
            test.Context.Tiles.Selection.RemoveAll();
            foreach (var tile in Tilemap.GetSpiralCenteredAt(Tile.Origin, 3))
            {
                test.Context.Tiles.Selection.Add(tile);
            }

            behaviorWithParameters(4).Generate(test.Context);

            var corners = Directions.All.Enumerate()
                .Select(d => Tile.Origin.Neighbor(d).Neighbor(d).Neighbor(d))
                .ToHashSet();
            foreach (var tile in test.Context.Tiles.All)
            {
                var tileSelected = test.Context.Tiles.Selection.Contains(tile);
                var isCorner = corners.Contains(tile);

                tileSelected.Should().Be(tile.Radius <= 3 && !isCorner);
            }
        }
    }
}
