using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("setTiles")]
sealed class SetTiles : NodeBehavior<SetTiles.BehaviorParameters>
{
    public record BehaviorParameters(TileType Type);

    public SetTiles(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        var geometry = new TileGeometry(Parameters.Type, 0, Unit.Zero);

        foreach (var tile in context.Tiles.Selection)
        {
            context.Tiles.Set(tile, geometry);
        }
    }
}