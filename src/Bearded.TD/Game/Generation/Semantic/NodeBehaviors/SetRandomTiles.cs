using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("setRandomTiles")]
    sealed class SetRandomTiles : NodeBehavior<SetRandomTiles.BehaviorParameters>
    {
        public sealed record BehaviorParameters(double Percentage, TileType Type);

        public SetRandomTiles(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            foreach (var t in context.Tiles.All)
            {
                if (!context.Random.NextBool(Parameters.Percentage))
                    continue;

                context.Tiles.Set(t, new TileGeometry(Parameters.Type, 0, 0.U()));
            }
        }
    }
}
