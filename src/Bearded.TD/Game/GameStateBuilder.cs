using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game
{
    static class GameStateBuilder
    {
        public static GameState Empty(GameMeta meta)
        {
            var tilemap = new Tilemap<TileInfo>(Constants.Game.World.Radius);
            foreach (var tile in tilemap)
            {
                tilemap[tile] = new TileInfo(tile.NeigbourDirectionsFlags, TileInfo.Type.Floor);
            }

            var gameState = new GameState(meta, new Level(tilemap));
            gameState.Add(new Base(new Tile<TileInfo>(tilemap, 0, 0)));

            return gameState;
        }

        public static GameState Generate(GameMeta meta, ITilemapGenerator generator)
        {
            var tilemap = new Tilemap<TileInfo>(Constants.Game.World.Radius);
            foreach (var tile in tilemap)
            {
                tilemap[tile] = new TileInfo(tile.NeigbourDirectionsFlags, TileInfo.Type.Floor);
            }
            generator.Fill(tilemap);

            var gameState = new GameState(meta, new Level(tilemap));
            gameState.Add(new Base(new Tile<TileInfo>(tilemap, 0, 0)));

            return gameState;
        }
    }
}
