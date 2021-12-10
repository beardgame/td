using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("createSpawnLocation")]
sealed class CreateSpawnLocation : NodeBehavior
{
    public override void Generate(NodeGenerationContext context)
    {
        var tileAtCenterOfNode = Level.GetTile(context.NodeData.Circles[0].Center);
        context.Content.PlaceSpawnLocation(tileAtCenterOfNode);
    }
}