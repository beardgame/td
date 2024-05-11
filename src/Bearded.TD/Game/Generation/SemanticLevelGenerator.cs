using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Logical;
using Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;

namespace Bearded.TD.Game.Generation;

sealed class SemanticLevelGenerator : ILevelGenerator
{
    private readonly Logger logger;
    private readonly LevelDebugMetadata metadata;

    public SemanticLevelGenerator(Logger logger, LevelDebugMetadata metadata)
    {
        this.logger = logger;
        this.metadata = metadata;
    }

    public IEnumerable<CommandFactory> Generate(LevelGenerationParameters parameters, int seed)
    {
        var radius = parameters.Radius;

        logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
        var timer = Stopwatch.StartNew();

        var random = new Random(seed);

        var nodeRadius = 5.U();

        var logicalTilemap = new LogicalTilemapGenerator(logger).Generate(parameters, random);

        // todo: adopt this into LogicalTilemapGenerator?
        drawLogicalNodes(logicalTilemap, nodeRadius);

        var commands = new PhysicalTilemapGenerator(metadata, nodeRadius, parameters.PropRules, logger)
            .Generate(logicalTilemap, random, radius);

        logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

        return commands;
    }

    private void drawLogicalNodes(LogicalTilemap logicalTilemap, Unit nodeRadius)
    {
        foreach (var tile in Tilemap.EnumerateTilemapWith(logicalTilemap.Radius))
        {
            var node = logicalTilemap[tile];

            var center = Position2.Zero + Level.GetPosition(tile).NumericValue * nodeRadius * 2;

            const float toOuterRadius = 2 / 1.73205080757f;
            foreach (var (direction, _) in node.MacroFeatures)
            {
                var before = center + direction.CornerBefore() * nodeRadius * toOuterRadius;
                var after = center + direction.CornerAfter() * nodeRadius * toOuterRadius;

                metadata.Add(new LineSegment(before, after, Color.Beige * 0.1f));
            }

            if (node.Blueprint == null)
                continue;
            metadata.Add(new Disk(
                center,
                nodeRadius, Color.Bisque * 0.05f
            ));
            foreach (var connectedDirection in node.ConnectedTo.Enumerate())
            {
                metadata.Add(new LineSegment(center, center + connectedDirection.Vector() * nodeRadius,
                    Color.Lime * 0.1f));
            }
        }
    }
}
