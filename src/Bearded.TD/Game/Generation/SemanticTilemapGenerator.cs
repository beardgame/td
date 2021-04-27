using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Debug;
using Bearded.TD.Game.Generation.Semantic;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.IO;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.World;
using static Bearded.TD.Game.Debug.LevelDebugMetadata;
using Tile = Bearded.TD.Tiles.Tile;

/*
 - get spawners and base from mod files somehow
    - decide based on mod files what nodes to add
        - nodes come from:
            - 'game rules/settings' in game mode or something - don't overcomplicate for now

    - nodes know how to put down spawners/base through node behaviours
 - node behaviours should become proper 'behaviours' and serialise from mod file

 */

namespace Bearded.TD.Game.Generation
{
    sealed class SemanticTilemapGenerator : ITilemapGenerator
    {
        private readonly Logger logger;
        private readonly LevelDebugMetadata metadata;

        public SemanticTilemapGenerator(Logger logger, LevelDebugMetadata metadata)
        {
            this.logger = logger;
            this.metadata = metadata;
        }

        public Tilemap<TileGeometry> Generate(int radius, int seed)
        {
            logger.Debug?.Log($"Started generating map with radius {radius} and seed {seed}");
            var timer = Stopwatch.StartNew();

            var random = new Random(seed);

            var area = Tilemap.TileCountForRadius(radius);
            var areaPerNode = 10 * 10;
            var nodeCount = area / areaPerNode / 2;
            var creviceCount = nodeCount;
            var nodeRadius = ((float) areaPerNode).Sqrted().U() * 0.5f;

            var logicalTilemap = new LogicalTilemapGenerator(logger).Generate(random, radius);

            var physicalTilemap = new PhysicalTilemapGenerator(logger, metadata)
                .Generate(logicalTilemap, random, radius, nodeRadius);

            logger.Debug?.Log($"Finished generating tilemap in {timer.Elapsed.TotalMilliseconds}ms");

            return physicalTilemap;
        }
    }
}
