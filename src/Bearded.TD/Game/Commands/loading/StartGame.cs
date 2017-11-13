using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class StartGame
    {
        public static ICommand<GameInstance> Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ICommand<GameInstance>
        {
            private readonly GameInstance game;

            public Implementation(GameInstance game)
            {
                this.game = game;
            }

            public void Execute()
            {
                game.Start();
            }

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}