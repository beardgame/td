﻿using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Commands
{
    static class BlockTilesForBuilding
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, IList<Tile<TileInfo>> tiles)
            => new Implementation(game, tiles);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly IList<Tile<TileInfo>> tiles;

            public Implementation(GameInstance game, IList<Tile<TileInfo>> tiles)
            {
                this.game = game;
                this.tiles = tiles;
            }

            public void Execute()
            {
                game.MustBeLoading();

                tiles.ForEach(t => t.Info.BlockForBuilding());
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(tiles);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private (int x, int y)[] tiles;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(IEnumerable<Tile<TileInfo>> tiles)
            {
                this.tiles = tiles.Select(tile => (tile.X, tile.Y)).ToArray();
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(
                        game,
                        tiles.Select(coords => new Tile<TileInfo>(game.State.Level.Tilemap, coords.x, coords.y)).ToList());

            public void Serialize(INetBufferStream stream)
            {
                stream.SerializeArrayCount(ref tiles);
                for (var i = 0; i < tiles.Length; i++)
                {
                    stream.Serialize(ref tiles[i].x);
                    stream.Serialize(ref tiles[i].y);
                }
            }
        }
    }
}
