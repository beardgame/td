using Bearded.TD.Game.Tilemap;
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
                tilemap[tile] = new TileInfo(Directions.All);
            }

            return new GameState(meta, new Level(tilemap));
        }
    }
}
