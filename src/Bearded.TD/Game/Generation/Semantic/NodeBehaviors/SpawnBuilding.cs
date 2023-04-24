using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;
using static SpawnBuilding.PlacementBias;

[NodeBehavior("spawnBuilding")]
sealed class SpawnBuilding : NodeBehavior<SpawnBuilding.BehaviorParameters>
{
    public SpawnBuilding(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        var rootTile = getRootTile(context);
        context.Content.PlaceBuilding(Parameters.Building, rootTile, Parameters.Faction);
    }

    private Tile getRootTile(NodeGenerationContext context)
    {
        return Parameters.Placement switch
        {
            Default => context.Tiles.Selection.Count > 0
                ? context.Tiles.Selection.First()
                : Level.GetTile(context.NodeData.Circles[0].Center),

            AwayFromConnections => validRootTiles(context)
                .MaxBy(t => context.NodeData.Connections.Sum(c => c.DistanceTo(t))),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IEnumerable<Tile> validRootTiles(NodeGenerationContext context)
    {
        var footprint = Parameters.Building.GetFootprint();

        var area = context.Tiles.All;

        return area.Where(t => footprint.Positioned(t, Orientation.Default).OccupiedTiles.All(area.Contains));
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(IGameObjectBlueprint Building, ExternalId<Faction> Faction, PlacementBias Placement);

    public enum PlacementBias
    {
        Default = 0,
        AwayFromConnections,
    }
}
