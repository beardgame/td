using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameState.GameLoop;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands.GameLoop
{
    static class WakeUpSpawnLocation
    {
        public static ISerializableCommand<GameInstance> Command(SpawnLocation spawnLocation)
            => new Implementation(spawnLocation);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly SpawnLocation spawnLocation;

            public Implementation(SpawnLocation spawnLocation)
            {
                this.spawnLocation = spawnLocation;
            }

            public void Execute()
            {
                spawnLocation.WakeUp();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(spawnLocation);
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<SpawnLocation> spawnLocation;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(SpawnLocation spawnLocation)
            {
                this.spawnLocation = spawnLocation.Id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State.Find(spawnLocation));

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref spawnLocation);
            }
        }
    }
}

