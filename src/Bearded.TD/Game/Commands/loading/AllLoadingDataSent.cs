using Bearded.TD.Commands;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Commands
{
    static class AllLoadingDataSent
    {
        public static ICommand Command(GameInstance game)
            => new Implementation(game);

        private class Implementation : ICommand
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

            public ICommandSerializer Serializer => new Serializer();
        }

        private class Serializer : ICommandSerializer
        {
            // ReSharper disable once UnusedMember.Local
            public Serializer()
            {
            }

            public ICommand GetCommand(GameInstance game)
                => new Implementation(game);

            public void Serialize(INetBufferStream stream)
            {
            }
        }
    }
}
