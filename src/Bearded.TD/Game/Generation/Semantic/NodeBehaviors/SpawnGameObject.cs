using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

using static ObjectPlacement;
using static ObjectPositioning;

[NodeBehavior("spawnGameObject")]
sealed class SpawnGameObject : NodeBehavior<SpawnGameObject.BehaviorParameters>
{
    public SpawnGameObject(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        foreach (var location in getLocations(context))
        {
            var direction = Parameters.Rotation.ToDirection(context.Random);
            context.Content.PlaceGameObject(Parameters.Blueprint, location, direction);
        }
    }

    private IEnumerable<Position3> getLocations(NodeGenerationContext context)
    {
        return Parameters.Placement
            .ToTiles(context, Parameters.Count, tileValidForFootprintPredicate(context))
            .Select(tile => Parameters.Alignment.ToPosition(tile, context.Random).WithZ(Parameters.Z));
    }


    private Predicate<Tile> tileValidForFootprintPredicate(NodeGenerationContext context)
    {
        var footprint = Parameters.Blueprint.GetFootprint();
        var area = context.Tiles.All;
        return tile => footprint.Positioned(tile, Orientation.Default).OccupiedTiles.All(area.Contains);
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(
        IGameObjectBlueprint Blueprint,
        PlacementMode Placement,
        AlignmentMode Alignment,
        RotationMode Rotation,
        Unit Z,
        int? Count);
}
