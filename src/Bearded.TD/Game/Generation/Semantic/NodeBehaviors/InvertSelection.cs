using Bearded.TD.Game.Generation.Semantic.Features;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("invertSelection")]
sealed class InvertSelection : NodeBehavior
{
    public override void Generate(NodeGenerationContext context)
    {
        foreach (var tile in context.Tiles.All)
        {
            if (context.Tiles.Selection.Contains(tile))
            {
                context.Tiles.Selection.Remove(tile);
            }
            else
            {
                context.Tiles.Selection.Add(tile);
            }
        }
    }
}