using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class GameOver
    {
        public static ICommand Command(GameState game)
            => new Implementation(game);

        private class Implementation : ICommand
        {
            private readonly GameState game;

            public Implementation(GameState game)
            {
                this.game = game;
            }

            public void Execute() => game.Meta.DoGameOver();

            public ICommandSerializer Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game.State);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}