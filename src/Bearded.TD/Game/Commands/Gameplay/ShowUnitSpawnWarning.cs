using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class ShowUnitSpawnWarning
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Id<UnitWarning> id, Tile tile)
            => new Implementation(game, id, tile);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Id<UnitWarning> id;
            private readonly Tile tile;

            public Implementation(GameInstance game, Id<UnitWarning> id, Tile tile)
            {
                this.game = game;
                this.id = id;
                this.tile = tile;
            }

            public void Execute()
            {
                game.State.Add(new UnitWarning(id, tile));
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(id, tile);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<UnitWarning> id;
            private int tileX;
            private int tileY;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Id<UnitWarning> id, Tile tile)
            {
                this.id = id;
                tileX = tile.X;
                tileY = tile.Y;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, id, new Tile(tileX, tileY));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
                stream.Serialize(ref tileX);
                stream.Serialize(ref tileY);
            }
        }
    }
}
