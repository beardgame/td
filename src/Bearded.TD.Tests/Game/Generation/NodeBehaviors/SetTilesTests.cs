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
    public class SetTilesTests
    {
        private INodeBehavior<Node> behaviourWithParameters(TileType type)
            => new SetTiles(new SetTiles.BehaviorParameters(type));

        public SetTilesTests()
        {
            Arb.Register<TilemapGenerators>();
        }

        [Property]
        public void MakesNoChangesWithEmptyNode(TileType type)
        {
            var test = TestContext.CreateEmpty();

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property]
        public void SetsAllTilesInNodeToGivenType(TileType type)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            foreach (var tile in test.Context.Tiles.All)
            {
                test.ExpectedTilemap[tile] = new TileGeometry(type, 0, 0.U());
            }

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property]
        public void SetsNoTilesForEmptySelection(TileType type)
        {
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
            test.Context.Tiles.Selection.RemoveAll();

            behaviourWithParameters(type)
                .Generate(test.Context);

            test.AssertSubjectTilemapEqualsExpectedTilemap();
        }

        [Property]
        public void SetsExactlyThoseTilesInSelection(TileType type, int seed)
        {
            var random = new Random(seed);
            var test = TestContext.CreateForHexagonalNodeWithRadius(2);
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
