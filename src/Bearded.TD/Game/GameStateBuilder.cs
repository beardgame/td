using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    static class GameStateBuilder
    {
        public static GameState Empty(GameMeta meta)
        {
            var tilemap = new Tilemap<TileInfo>(Constants.Game.World.Radius);
            foreach (var tile in tilemap)
            {
                tilemap[tile] = new TileInfo(
                    tile.Radius < tilemap.Radius
                        ? Directions.All
                        : getValidDirections(tile),
                    TileInfo.Type.Floor);
            }

            var gameState = new GameState(meta, new Level(tilemap));
            gameState.Add(new Base(Footprint.CircleSeven.Positioned(gameState.Level, new Position2())));

            return gameState;
        }

        private static Directions getValidDirections(Tile<TileInfo> tile)
        {
            return Tiles.Tilemap.Directions
                .Where((d) => tile.Neighbour(d).IsValid)
                .Aggregate(Directions.None, (ds, d) => ds.And(d));
        }
    }
}
