using System;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using FsCheck;
using FsCheck.Xunit;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SetTilesTests
    {
        private INodeBehavior behaviourWithParameters(TileType type)
            => new SetTiles(new SetTiles.BehaviorParameters(type));

        [Property(Arbitrary = new[] { typeof(TilemapGenerators) })]
        public void MakesNoChangesWithEmptyNode(TileType type)
        {
            var test = GenerationTestContext.CreateEmpty();

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property(Arbitrary = new[] { typeof(TilemapGenerators) })]
        public void SetsAllTilesInNodeToGivenType(TileType type)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.ExpectedTilemap[tile] = new TileGeometry(type, 0, 0.U());
            }

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property(Arbitrary = new[] { typeof(TilemapGenerators) })]
        public void SetsNoTilesForEmptySelection(TileType type)
        {
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property(Arbitrary = new[] { typeof(TilemapGenerators) })]
        public void SetsExactlyThoseTilesInSelection(TileType type, int seed)
        {
            var random = new Random(seed);
            var test = GenerationTestContext.CreateForHexagonalNodeWithRadius(2);
            foreach (var tile in test.Context.Tiles.All)
            {
                if (random.NextBool())
                    test.Context.Tiles.Selection.Remove(tile);
            }
            foreach (var tile in test.Context.Tiles.Selection)
            {
                test.ExpectedTilemap[tile] = new TileGeometry(type, 0, 0.U());
            }

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }
    }
}
