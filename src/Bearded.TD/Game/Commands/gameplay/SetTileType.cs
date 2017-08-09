using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;

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

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(tile, type);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private int x;
            private int y;
            private TileInfo.Type type;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Tile<TileInfo> tile, TileInfo.Type type)
            {
                x = tile.X;
                y = tile.Y;
                this.type = type;
            }
            
            protected override UnifiedRequestCommand GetSerialized(GameInstance game, Player player) =>
                new Implementation(
                    game.State,
                    new Tile<TileInfo>(game.State.Level.Tilemap, x, y),
                    type
                );

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref x);
                stream.Serialize(ref y);
                var t = (byte) type;
                stream.Serialize(ref t);
                type = (TileInfo.Type) t;
            }
        }
    }
}