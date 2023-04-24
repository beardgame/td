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
using Extensions = Bearded.Utilities.Linq.Extensions;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors;

using static SpawnGameObject.AlignmentMode;
using static SpawnGameObject.PlacementMode;

[NodeBehavior("spawnGameObject")]
sealed class SpawnGameObject : NodeBehavior<SpawnGameObject.BehaviorParameters>
{
    public SpawnGameObject(BehaviorParameters parameters) : base(parameters) { }

    public override void Generate(NodeGenerationContext context)
    {
        var location = getLocation(context);
        context.Content.PlaceGameObject(Parameters.Blueprint, location, Direction2.Zero);
    }

    private Position3 getLocation(NodeGenerationContext context)
    {
        var tile = Parameters.Placement switch
        {
            FirstFromSelection => context.Tiles.Selection.Count > 0
                ? context.Tiles.Selection.First()
                : Level.GetTile(context.NodeData.Circles[0].Center),

            AwayFromConnections => validTiles(context)
                .MaxBy(t => context.NodeData.Connections.Sum(c => c.DistanceTo(t))),

            RandomTile => Extensions.RandomElement(context.Tiles.Selection, context.Random),

            _ => throw new ArgumentOutOfRangeException($"Unhandled placement mode: {Parameters.Placement}")
        };

        return getPositionWithinTile(tile, context);
    }

    private IEnumerable<Tile> validTiles(NodeGenerationContext context)
    {
        var footprint = Parameters.Blueprint.GetFootprint();

        var area = context.Tiles.All;

        return area.Where(t => footprint.Positioned(t).OccupiedTiles.All(area.Contains));
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
        IGameObjectBlueprint Blueprint, PlacementMode Placement, AlignmentMode Alignment, Unit Z);

    public enum PlacementMode
    {
        FirstFromSelection = 0,
        AwayFromConnections,
        RandomTile,
    }

    public enum AlignmentMode
    {
        CenterOfTile = 0,
        RandomlyInTile,
    }
}
