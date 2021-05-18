using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("fillCenterWithWalls")]
    sealed class FillCenterWithWalls : NodeBehavior<FillCenterWithWalls.BehaviorParameters>
    {
        public record BehaviorParameters(int TilesFromEdge);

        public FillCenterWithWalls(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            var tiles = new Tiles.Tiles(context.Tiles);
            foreach (var _ in Enumerable.Range(0, Parameters.TilesFromEdge))
            {
                tiles = tiles.Erode();
            }

            foreach (var tile in tiles)
            {
                context.Set(tile, new TileGeometry(TileType.Wall, 1, 0.U()));
            }
        }
    }
}
