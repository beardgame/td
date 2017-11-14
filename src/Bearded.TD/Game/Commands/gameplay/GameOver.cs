using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands
{
    static class GameOver
    {
        public static ICommand<GameInstance> Command(GameState game)
            => new Implementation(game);

        private class Implementation : ICommand<GameInstance>
        {
            private readonly GameState game;

            public Implementation(GameState game)
            {
                this.game = game;
            }

            public void Execute() => game.Meta.DoGameOver();

            public ICommandSerializer<GameInstance> Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer<GameInstance>
        {
            public Serializer() { }

            public ICommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State);

            public void Serialize(INetBufferStream stream) { }
        }
    }
}