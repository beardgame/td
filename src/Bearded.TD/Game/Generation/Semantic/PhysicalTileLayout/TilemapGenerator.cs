using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

static partial class TilemapGenerator
{
    public static (Tilemap<TileGeometry> Tilemap, ImmutableArray<CommandFactory> Commands) GenerateTilemap(
        int radius, IEnumerable<TiledFeature> features, Tilemap<IBiome> biomes, Random random)
    {
        var commandAccumulator = new LevelGenerationCommandAccumulator();

        var tilemap = generateFeatures(radius, features, commandAccumulator, random);
        generateProps(tilemap, biomes, commandAccumulator, random);

        return (tilemap, commandAccumulator.ToCommandFactories());
    }

    private static void generateProps(
        Tilemap<TileGeometry> tilemap,
        Tilemap<IBiome> biomes,
        LevelGenerationCommandAccumulator commandAccumulator,
        Random random)
    {
        var context = new PropGenerationContext(tilemap, biomes, commandAccumulator, random);

        // TODO: do something that actually generates the props
    }
}
