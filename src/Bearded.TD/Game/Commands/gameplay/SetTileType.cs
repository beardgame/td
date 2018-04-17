using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Commands
{
    static class SetTileType
    {
        public static IRequest<GameInstance> Request(GameState game, Tile<TileInfo> tile, TileInfo.Type type, TileDrawInfo drawInfo)
            => new Implementation(game, tile, type, drawInfo);

        private class Implementation : UnifiedDebugRequestCommand
        {
            private readonly GameState game;
            private readonly Tile<TileInfo> tile;
            private readonly TileInfo.Type type;
            private readonly TileDrawInfo drawInfo;

            public Implementation(GameState game, Tile<TileInfo> tile, TileInfo.Type type, TileDrawInfo drawInfo)
            {
                this.game = game;
                this.tile = tile;
                this.type = type;
                this.drawInfo = drawInfo;
            }

            protected override bool CheckPreconditionsDebug() => tile.IsValid;

            public override void Execute() => game.Geometry.SetTileType(tile, type, drawInfo);

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(tile, type, drawInfo);
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private int x;
            private int y;
            private TileInfo.Type type;
            private Unit height;
            private float hexScale;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Tile<TileInfo> tile, TileInfo.Type type, TileDrawInfo drawInfo)
            {
                x = tile.X;
                y = tile.Y;
                this.type = type;
                height = drawInfo.Height;
                hexScale = drawInfo.HexScale;
            }
            
            protected override UnifiedRequestCommand GetSerialized(GameInstance game, Player player) =>
                new Implementation(
                    game.State,
                    new Tile<TileInfo>(game.State.Level.Tilemap, x, y),
                    type,
                    new TileDrawInfo(height, hexScale)
                );

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref x);
                stream.Serialize(ref y);
                var t = (byte) type;
                stream.Serialize(ref t);
                type = (TileInfo.Type) t;
                stream.Serialize(ref height);
                stream.Serialize(ref hexScale);
            }
        }
    }
}