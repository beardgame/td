﻿using Bearded.TD.Commands;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Commands
{
    static class ShowUnitSpawnWarning
    {
        public static ICommand Command(GameInstance game, Tile<TileInfo> tile, Instant until)
            => new Implementation(game, tile, until);

        private class Implementation : ICommand
        {
            private readonly GameInstance game;
            private readonly Tile<TileInfo> tile;
            private readonly Instant until;

            public Implementation(GameInstance game, Tile<TileInfo> tile, Instant until)
            {
                this.tile = tile;
                this.until = until;
                this.game = game;
            }

            public void Execute()
            {
                game.State.Add(new UnitWarning(tile, until));
            }

            public ICommandSerializer Serializer => new Serializer(tile, until);
        }

        private class Serializer : ICommandSerializer
        {
            private int tileX;
            private int tileY;
            private double until;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Tile<TileInfo> tile, Instant until)
            {
                tileX = tile.X;
                tileY = tile.Y;
                this.until = until.NumericValue;
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game, new Tile<TileInfo>(game.State.Level.Tilemap, tileX, tileY), new Instant(until));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref tileX);
                stream.Serialize(ref tileY);
                stream.Serialize(ref until);
            }
        }
    }
}
