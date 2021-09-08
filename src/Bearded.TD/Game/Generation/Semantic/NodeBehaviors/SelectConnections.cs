using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("selectConnections")]
    sealed class SelectConnections : NodeBehavior
    {
        public override void Generate(NodeGenerationContext context)
        {
            context.Tiles.Selection.RemoveAll();

            foreach (var tile in context.NodeData.Connections)
            {
                context.Tiles.Selection.Add(tile);
            }
        }
    }
}
