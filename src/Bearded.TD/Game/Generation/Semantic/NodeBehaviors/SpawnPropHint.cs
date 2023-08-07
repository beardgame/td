using System.Collections.Generic;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

using static ObjectPlacement;

[NodeBehavior("spawnPropHint")]
sealed class SpawnPropHint : NodeBehavior<SpawnPropHint.BehaviorParameters>
{
    public SpawnPropHint(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        foreach (var tile in getTiles(context))
        {
            context.Content.PlacePropHint(tile, Parameters.Purpose);
        }
    }

    private IEnumerable<Tile> getTiles(NodeGenerationContext context)
    {
        return Parameters.Placement.ToTiles(context, Parameters.Count, null);
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(
        PlacementMode Placement,
        PropPurpose Purpose,
        int? Count);
}
