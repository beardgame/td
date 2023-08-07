using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

using static ObjectPlacement;
using static SpawnGameObject.AlignmentMode;
using static SpawnGameObject.RotationMode;

[NodeBehavior("spawnGameObject")]
sealed class SpawnGameObject : NodeBehavior<SpawnGameObject.BehaviorParameters>
{
    public SpawnGameObject(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        foreach (var location in getLocations(context))
        {
            var direction = getDirection(context);
            context.Content.PlaceGameObject(Parameters.Blueprint, location, direction);
        }
    }

    private IEnumerable<Position3> getLocations(NodeGenerationContext context)
    {
        return Parameters.Placement
            .ToTiles(context, Parameters.Count, tileValidForFootprintPredicate(context))
            .Select(tile => getPositionWithinTile(tile, context));
    }

    private Direction2 getDirection(NodeGenerationContext context)
    {
        return Parameters.Rotation switch
        {
            FixedDirection => Direction2.Zero,
            RandomDirection => Direction2.FromDegrees(context.Random.NextSingle() * 360),
            _ => throw new ArgumentOutOfRangeException($"Unhandled rotation mode: {Parameters.Rotation}")
        };
    }

    private Predicate<Tile> tileValidForFootprintPredicate(NodeGenerationContext context)
    {
        var footprint = Parameters.Blueprint.GetFootprint();
        var area = context.Tiles.All;
        return tile => footprint.Positioned(tile, Orientation.Default).OccupiedTiles.All(area.Contains);
    }

    private Position3 getPositionWithinTile(Tile tile, NodeGenerationContext context)
    {
        var pos2 = Parameters.Alignment switch
        {
            CenterOfTile => Level.GetPosition(tile),
            RandomlyInTile => Level.GetPosition(tile) +
                GeometricRandom.UniformRandomPointOnDisk(context.Random, Constants.Game.World.HexagonInnerRadius),
            _ => throw new ArgumentOutOfRangeException($"Unhandled alignment mode: {Parameters.Alignment}")
        };
        return pos2.WithZ(Parameters.Z);
    }

    [UsedImplicitly]
    public sealed record BehaviorParameters(
        IGameObjectBlueprint Blueprint,
        PlacementMode Placement,
        AlignmentMode Alignment,
        RotationMode Rotation,
        Unit Z,
        int? Count);

    public enum AlignmentMode
    {
        CenterOfTile = 0,
        RandomlyInTile,
    }

    public enum RotationMode
    {
        FixedDirection = 0,
        RandomDirection,
    }
}
