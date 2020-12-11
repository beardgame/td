using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Commands.Loading
{
    static class BeginLoadingGame
    {
        public static ISerializableCommand<GameInstance> Command(GameInstance game)
            => new Implementation(game);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                game.SetLoading();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            [UsedImplicitly]
            public Serializer() {}

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}
