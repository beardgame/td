using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Commands
{
    static class SetTileType
    {
        public static IRequest Request(GameState game, Tile<TileInfo> tile, TileInfo.Type type)
            => new Implementation(game, tile, type);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameState game;
            private readonly Tile<TileInfo> tile;
            private readonly TileInfo.Type type;

            public Implementation(GameState game, Tile<TileInfo> tile, TileInfo.Type type)
            {
                this.game = game;
                this.tile = tile;
                this.type = type;
            }

#if DEBUG
            public override bool CheckPreconditions() => tile.IsValid;
#else
            public override bool  CheckPreconditions() => false;
#endif

            public override void Execute() => game.Geometry.SetTileType(tile, type);
        }
    }
}