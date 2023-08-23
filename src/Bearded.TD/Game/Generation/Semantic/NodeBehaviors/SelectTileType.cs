using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("selectTileType")]
sealed class SelectTileType : NodeBehavior<SelectTileType.BehaviorParameters>
{
    public sealed record BehaviorParameters(TileType Type);

    public SelectTileType(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        context.Tiles.Selection.RemoveAll();

        var tilesOfType = context.Tiles.All.Where(t => context.Tiles.Get(t).Type == Parameters.Type);

        foreach (var tile in tilesOfType)
        {
            context.Tiles.Selection.Add(tile);
        }
    }
}
