using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game
{
    static class GameStateBuilder
    {
        const int levelRadius = 20;

        public static GameState Empty(GameMeta meta)
        {
            var tilemap = new Tilemap<TileInfo>(levelRadius);
            foreach (var tile in tilemap)
            {
                tilemap[tile] = new TileInfo(
                    tile.Radius < tilemap.Radius
                        ? Directions.All
                        : getValidDirections(tile));
            }

            return new GameState(meta, new Level(tilemap));
        }

        private static Directions getValidDirections(Tile<TileInfo> tile)
        {
            return Tiles.Tilemap.Directions
                .Where((d) => tile.Neighbour(d).IsValid)
                .Aggregate(Directions.None, (ds, d) => ds.And(d));
        }
    }
}
