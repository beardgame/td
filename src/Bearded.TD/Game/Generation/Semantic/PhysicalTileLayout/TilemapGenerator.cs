using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Generation.Semantic.Props;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout;

static partial class TilemapGenerator
{
    public static (Tilemap<TileGeometry> Tilemap, ImmutableArray<CommandFactory> Commands) GenerateTilemap(
        int radius,
        IEnumerable<TiledFeature> features,
        Tilemap<IBiome> biomes,
        IEnumerable<IPropRule> propRules,
        Logger logger,
        Random random)
    {
        var commandAccumulator = new LevelGenerationCommandAccumulator();
        var propHintAccumulator = new Accumulator<PropHint>();

        var tilemap = generateFeatures(radius, features, commandAccumulator, propHintAccumulator, random);
        generateProps(
            tilemap,
            biomes,
            propRules,
            propHintAccumulator.Consume(all => all.ToImmutableArray()),
            commandAccumulator,
            logger,
            random);

        return (tilemap, commandAccumulator.ToCommandFactories());
    }

    private static void generateProps(
        Tilemap<TileGeometry> tilemap,
        Tilemap<IBiome> biomes,
        IEnumerable<IPropRule> propRules,
        IEnumerable<PropHint> propHints,
        LevelGenerationCommandAccumulator commandAccumulator,
        Logger logger,
        Random random)
    {
        var solver = new PropSolver(logger);
        var hintsArray = propHints.ToImmutableArray();
        var context = new PropGenerationContext(tilemap, biomes, hintsArray, solver, random);
        foreach (var rule in propRules)
        {
            rule.Execute(context);
        }

        var contentContext = new PropContentGenerationContext(commandAccumulator);
        solver.CommitSolutions(hintsArray, contentContext, random);
    }
}
