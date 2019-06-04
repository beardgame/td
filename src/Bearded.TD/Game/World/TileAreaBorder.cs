using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    class TileAreaBorder
    {
        private readonly List<(Tile, Direction)> edges;

        private TileAreaBorder(List<(Tile, Direction)> edges)
        {
            this.edges = edges;
        }

        public static TileAreaBorder From(Level level, Func<Tile, bool> predicate)
            => From(Tilemap.EnumerateTilemapWith(level.Radius), predicate);

        public static TileAreaBorder From(IEnumerable<Tile> area, Func<Tile, bool> predicate)
            => From(area.Where(predicate));

        public static TileAreaBorder From(IEnumerable<Tile> area)
            => From(new HashSet<Tile>(area));
        
        public static TileAreaBorder From(HashSet<Tile> area)
        {
            var edges = area.SelectMany(
                    tile => Extensions.Directions
                        .Where(direction => !area.Contains(tile.Neighbour(direction)))
                        .Select(direction => (tile, direction))
                )
                .ToList();
            
            return new TileAreaBorder(edges);
        }

        public void Visit(Action<(Tile Tile, Direction Direction)> visitBorder)
        {
            edges.ForEach(visitBorder);
        }
    }
}
