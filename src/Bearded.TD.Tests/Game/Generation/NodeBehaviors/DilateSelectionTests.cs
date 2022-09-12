using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Tiles;
using FluentAssertions;
using Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class DilateSelectionTests
    {
        [Fact]
        public void DilatingFullSelectionDoesNothing()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);

            new DilateSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(test.Context.Tiles.All.Count);
        }

        [Fact]
        public void DilatingEmptySelectionDoesNothing()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            new DilateSelection().Generate(test.Context);

            test.Context.Tiles.Selection.Count.Should().Be(0);
        }

        [Fact]
        public void DilatingSingleTileSelectsSurroundingHexagon()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(1, 1));

            new DilateSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = test.Context.Tiles.Selection.Contains(tile);

                var inHexagon = tile.DistanceTo(new Tile(1, 1)) <= 1;

                selected.Should().Be(inHexagon);
            }
        }

        [Fact]
        public void DilatingLineSelectsThickerLine()
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();
            test.Context.Tiles.Selection.Add(new Tile(0, 0));
            test.Context.Tiles.Selection.Add(new Tile(1, 0));
            test.Context.Tiles.Selection.Add(new Tile(2, 0));

            new DilateSelection().Generate(test.Context);

            foreach (var tile in test.Context.Tiles.All)
            {
                var selected = test.Context.Tiles.Selection.Contains(tile);

                var inThickLine =
                    tile.DistanceTo(new Tile(0, 0)) <= 1 ||
                    tile.DistanceTo(new Tile(1, 0)) <= 1 ||
                    tile.DistanceTo(new Tile(2, 0)) <= 1;

                selected.Should().Be(inThickLine);
            }
        }
    }
}
