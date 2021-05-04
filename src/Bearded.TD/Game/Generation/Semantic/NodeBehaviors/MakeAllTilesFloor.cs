using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("makeAllTilesFloor")]
    sealed class MakeAllTilesFloor : NodeBehavior
    {
        public override void Generate(NodeGenerationContext context)
        {
            foreach (var t in context.Tiles)
            {
                context.Set(t, new TileGeometry(TileType.Floor, 0, 0.U()));
            }
        }
    }
}
