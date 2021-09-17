using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Game.Commands.Gameplay
{
    static class GameOver
    {
        public static ISerializableCommand<GameInstance> Command(GameState game)
            => new Implementation(game);

        private sealed class Implementation : ISerializableCommand<GameInstance>
        {
            private readonly GameState game;

            public Implementation(GameState game)
            {
                this.game = game;
            }

            public void Execute() => game.Meta.DoGameOver();

            ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer();
        }

        private sealed class Serializer : ICommandSerializer<GameInstance>
        {
            // ReSharper disable once EmptyConstructor
            public Serializer() { }

            public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
                => new Implementation(game.State);

            public void Serialize(INetBufferStream stream) { }
        }
    }
}
