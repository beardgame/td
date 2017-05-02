using System.Linq;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Lobby;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game
{
    static class FinishLoadingGame
    {
        public static ICommand Command(GameInstance game, Player player)
            => new Implementation(game, player);
        public static IRequest Request(GameInstance game, Player player)
            => new Implementation(game, player);

        private class Implementation : UnifiedRequestCommand
        {
            private readonly GameInstance game;
            private readonly Player player;

            public Implementation(GameInstance game, Player player)
            {
                this.game = game;
                this.player = player;
            }

            public override bool CheckPreconditions()
            {
                return player.ConnectionState == PlayerConnectionState.Loading && game.Status == GameStatus.Loading;
            }

            public override void Execute()
            {
                player.ConnectionState = PlayerConnectionState.FinishedLoading;
                game.State.Meta.Dispatcher.RunOnlyOnServer(checkForAllPlayersFinished);
            }

            protected override UnifiedRequestCommandSerializer GetSerializer() => new Serializer(player);

            private void checkForAllPlayersFinished(ICommandDispatcher commandDispatcher)
            {
                if (game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
                    commandDispatcher.Dispatch(StartGame.Command(game));
            }
        }

        private class Serializer : UnifiedRequestCommandSerializer
        {
            private Id<Player> player;

            // ReSharper disable once UnusedMember.Local
            public Serializer() { }

            public Serializer(Player player)
            {
                this.player = player.Id;
            }

            protected override UnifiedRequestCommand GetSerialized(GameInstance game)
            {
                return new Implementation(game, game.PlayerFor(player));
            }

            public override void Serialize(INetBufferStream stream)
            {
                stream.Serialize(ref player);
            }
        }
    }
}