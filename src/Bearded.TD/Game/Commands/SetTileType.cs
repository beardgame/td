using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;

namespace Bearded.TD.Game.Commands
{
    static class SetTileType
    {
        public static IRequest Request(GameState game, Tile<TileInfo> tile, TileInfo.Type type)
            => new Implementation(game, tile, type);

        private class Implementation : UnifiedDebugRequestCommand
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

            protected override bool CheckPreconditionsDebug() => tile.IsValid;

            public override void Execute() => game.Geometry.SetTileType(tile, type);
            protected override IUnifiedRequestCommandSerializer GetSerializer()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}