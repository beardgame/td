using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities.Collections;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Loading;

static class AllLoadingDataSent
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
            game.State.FinishLoading();
            game.Players.ForEach(p => p.ConnectionState = PlayerConnectionState.ProcessingLoadingData);
        }

        ICommandSerializer<GameInstance> ISerializableCommand<GameInstance>.Serializer => new Serializer();
    }

    private sealed class Serializer : ICommandSerializer<GameInstance>
    {
        [UsedImplicitly]
        public Serializer() {}

        public ISerializableCommand<GameInstance> GetCommand(GameInstance game)
            => new Implementation(game);

        public void Serialize(INetBufferStream stream) {}
    }
}