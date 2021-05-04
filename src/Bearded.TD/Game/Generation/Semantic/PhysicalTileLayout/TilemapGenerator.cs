using System;
using System.Collections.Generic;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Generation.Semantic.PhysicalTileLayout
{
    sealed class TilemapGenerator
    {
        public Tilemap<TileGeometry> GenerateTilemap(int radius, IEnumerable<TiledFeature> features)
        {
            var tilemap = new Tilemap<TileGeometry>(radius, _ => new TileGeometry(TileType.Wall, 1, Unit.Zero));

            foreach (var (feature, tiles) in features)
            {
                switch (feature)
                {
                    case PhysicalFeature.Connection connection:
                        foreach (var tile in tiles)
                        {
                            tilemap[tile] = new TileGeometry(TileType.Floor, 1, Unit.Zero);
                        }

                        break;
                    case PhysicalFeature.Crevice crevice:
                        foreach (var tile in tiles)
                        {
                            tilemap[tile] = new TileGeometry(TileType.Crevice, 1, -5.U());
                        }

                        break;
                    case PhysicalFeature.Node node:
                        var context = new NodeGenerationContext(tilemap, tiles);
                        foreach (var behavior in node.Blueprint.Behaviors)
                        {
                            behavior.Generate(context);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return tilemap;
        }
    }
}
