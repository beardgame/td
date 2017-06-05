using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class FillTilemap
    {
        public static ICommand Command(GameInstance game, Tilemap<TileInfo.Type> tilemap)
            => new Implementation(game, tilemap.Select(t => t.Info).ToList());

        private class Implementation : ICommand
        {
            private readonly Tilemap<TileInfo> tilemap;
            private readonly IList<TileInfo.Type> tiles;
            private readonly GameInstance game;

            public Implementation(GameInstance game, IList<TileInfo.Type> tiles)
            {
                this.game = game;
                tilemap = game.State.Level.Tilemap;
                this.tiles = tiles;
            }

            public void Execute()
            {
                game.MustBeLoading();

                foreach (var (tile, i) in tilemap.Select((t, i) => (t, i)))
                {
                    tile.Info.SetTileType(tiles[i]);
                }
            }

            public ICommandSerializer Serializer => new Serializer(tiles);
        }

        private class Serializer : ICommandSerializer
        {
            private TileInfo.Type[] tiles;

            public Serializer(IList<TileInfo.Type> tiles)
            {
                this.tiles = tiles.ToArray();
            }

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, tiles);

            public void Serialize(INetBufferStream stream)
            {
                var count = tiles?.Length ?? 0;
                stream.Serialize(ref count);
                if (tiles == null)
                    tiles = new TileInfo.Type[count];
                var bytes = (byte[]) (object) tiles;
                foreach (var i in Enumerable.Range(0, count))
                {
                    stream.Serialize(ref bytes[i]);
                }
            }
        }
    }
}
