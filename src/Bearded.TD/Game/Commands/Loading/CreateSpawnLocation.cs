using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Tiles;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.Loading
{
    static class CreateSpawnLocation
    {
        public static ISerializableCommand<GameInstance> Command(
                GameInstance game, Id<SpawnLocation> id, Tile tile)
            => new Implementation(game, id, tile);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Id<SpawnLocation> id;
            private readonly Tile tile;

            public Implementation(GameInstance game, Id<SpawnLocation> id, Tile tile)
            {
                this.game = game;
                this.id = id;
                this.tile = tile;
            }

            public void Execute()
            {
                game.State.Add(new SpawnLocation(id, tile));
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(id, tile);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<SpawnLocation> id;
            private int tileX;
            private int tileY;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Id<SpawnLocation> id, Tile tile)
            {
                this.id = id;
                (tileX, tileY) = tile;
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

