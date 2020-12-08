using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.GameState.Units;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class HideUnitSpawnWarning
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game, Id<UnitWarning> id)
            => new Implementation(game, id);

        private class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;
            private readonly Id<UnitWarning> id;

            public Implementation(GameInstance game, Id<UnitWarning> id)
            {
                this.game = game;
                this.id = id;
            }

            public void Execute()
            {
                game.State.Find(id).Delete();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer(id);
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            private Id<UnitWarning> id;

            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public Serializer(Id<UnitWarning> id)
            {
                this.id = id;
            }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game, id);

            public void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref id);
            }
        }
    }
}
