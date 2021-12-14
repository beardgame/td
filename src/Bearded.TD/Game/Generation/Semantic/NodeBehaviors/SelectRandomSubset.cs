using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("selectRandomSubset")]
sealed class SelectRandomSubset : NodeBehavior<SelectRandomSubset.BehaviorParameters>
{
    public sealed record BehaviorParameters(int NumTiles);

    public SelectRandomSubset(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        if (context.Tiles.Selection.Count <= Parameters.NumTiles)
        {
            return;
        }

        var tilesToSelect = context.Tiles.Selection
            .RandomSubset(Parameters.NumTiles, context.Random)
            .ToImmutableArray();

        context.Tiles.Selection.RemoveAll();
        foreach (var tile in tilesToSelect)
        {
            context.Tiles.Selection.Add(tile);
        }
    }
}