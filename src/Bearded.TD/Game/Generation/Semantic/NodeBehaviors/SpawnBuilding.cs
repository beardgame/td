using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

[NodeBehavior("spawnBuilding")]
sealed class SpawnBuilding : NodeBehavior<SpawnBuilding.BehaviorParameters>
{
    public SpawnBuilding(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        var rootTile = context.Tiles.Selection.Count > 0
            ? context.Tiles.Selection.First()
            : Level.GetTile(context.NodeData.Circles[0].Center);
        context.Content.PlaceBuilding(Parameters.Building, rootTile, Parameters.Faction);
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(IComponentOwnerBlueprint Building, ExternalId<Faction> Faction);
}