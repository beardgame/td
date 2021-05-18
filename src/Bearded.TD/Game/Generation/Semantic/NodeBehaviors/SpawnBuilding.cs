using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("spawnBuilding")]
    sealed class SpawnBuilding : NodeBehavior<SpawnBuilding.BehaviorParameters>
    {
        public SpawnBuilding(BehaviorParameters parameters) : base(parameters) { }

        public override void Generate(NodeGenerationContext context)
        {
            context.PlaceBuilding(Parameters.Building, Level.GetTile(context.Circles[0].Center));
        }

        public sealed record BehaviorParameters(IBuildingBlueprint Building);
    }
}
