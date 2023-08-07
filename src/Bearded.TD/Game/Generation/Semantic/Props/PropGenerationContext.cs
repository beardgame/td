using System;
using System.Collections.Generic;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Props;

sealed class PropGenerationContext
{
    private readonly Tilemap<TileGeometry> geometry;
    private readonly Tilemap<IBiome> biomes;
    private readonly LevelGenerationCommandAccumulator commandAccumulator;

    public Random Random { get; }

    public PropGenerationContext(
        Tilemap<TileGeometry> geometry,
        Tilemap<IBiome> biomes,
        LevelGenerationCommandAccumulator commandAccumulator,
        Random random)
    {
        this.geometry = geometry;
        this.biomes = biomes;
        Random = random;
        this.commandAccumulator = commandAccumulator;
    }

    public TileGeometry GeometryFor(Tile tile) => geometry[tile];
    public IBiome BiomeFor(Tile tile) => biomes[tile];

    public void PlaceGameObject(IGameObjectBlueprint blueprint, Position3 position, Direction2 direction)
    {
        commandAccumulator.PlaceGameObject(blueprint, position, direction);
    }

    public IEnumerable<PropHint> EnumerateHints(PropPurpose purpose)
    {
        yield break;
    }
}
