using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    static class GameStateBuilder
    {
        public static GameState Empty(GameMeta meta)
        {
            var tilemap = getEmptyTilemap(Constants.Game.World.Radius);
            return getGameStateFromTilemap(meta, tilemap);
        }

        public static GameState Generate(GameMeta meta, ITilemapGenerator generator)
        {
            var tilemap = getEmptyTilemap(Constants.Game.World.Radius);
            generator.Fill(tilemap);
            return getGameStateFromTilemap(meta, tilemap);
        }

        private static Tilemap<TileInfo> getEmptyTilemap(int radius)
        {
            var tilemap = new Tilemap<TileInfo>(radius);
            foreach (var tile in tilemap)
            {
                tilemap[tile] = new TileInfo(tile.NeigbourDirectionsFlags, TileInfo.Type.Floor);
            }
            return tilemap;
        }

        private static GameState getGameStateFromTilemap(GameMeta meta, Tilemap<TileInfo> tilemap)
        {
            var gameState = new GameState(meta, new Level(tilemap));
            gameState.Add(new Base(Footprint.CircleSeven.Positioned(gameState.Level, new Position2())));
            var center = new Tile<TileInfo>(tilemap, 0, 0);
            Directions.All.Enumerate()
                .Select((d) => center.Offset(d.Step() * tilemap.Radius))
                .ForEach((t) => gameState.Add(new UnitSource(t)));

            return gameState;
        }
    }
}
