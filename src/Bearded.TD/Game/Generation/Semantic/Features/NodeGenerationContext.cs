using System;
using System.Collections.Immutable;
using Bearded.TD.Game.Generation.Semantic.Commands;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.Features
{
    sealed record NodeGenerationData(ImmutableArray<Circle> Circles, ImmutableArray<Tile> Connections);

    sealed record NodeGenerationContext(
        Random Random,
        NodeGenerationData NodeData,
        NodeTileGenerationContext Tiles,
        NodeContentGenerationContext Content)
    {
        public static NodeGenerationContext Create(
            Tilemap<TileGeometry> tilemap,
            IArea tiles,
            ImmutableArray<Circle> circles,
            ImmutableArray<Tile> connections,
            LevelGenerationCommandAccumulator commandAccumulator,
            Random random)
        {
            return new NodeGenerationContext(
                random,
                new NodeGenerationData(circles, connections),
                new NodeTileGenerationContext(tilemap, tiles),
                new NodeContentGenerationContext(commandAccumulator, tiles)
                );
        }
    }
}
