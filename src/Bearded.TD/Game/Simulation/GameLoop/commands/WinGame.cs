using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.GameLoop;

static class WinGame
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

        public void Execute()
        {
            game.Meta.DoGameVictory();
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer();
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        [UsedImplicitly]
        public Serializer() { }

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game.State);

        public void Serialize(INetBufferStream stream) { }
    }
}