using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Loading
{
    class ServerLoadingManager : LoadingManager
    {
        public ServerLoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
            : base(game, dispatcher, networkInterface, logger)
        {
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            // Just instantly finish sending everything.
            if (Game.Me.ConnectionState == PlayerConnectionState.AwaitingLoadingData)
                Dispatcher.RunOnlyOnServer(
                    commandDispatcher => commandDispatcher.Dispatch(AllLoadingDataSent.Command(Game)));

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
                Game.RequestDispatcher.Dispatch(
                    ChangePlayerState.Request(Game.Me, PlayerConnectionState.FinishedLoading));

            // Check if all players finished loading and start the game if so.
            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.FinishedLoading))
                Dispatcher.RunOnlyOnServer(commandDispatcher => commandDispatcher.Dispatch(StartGame.Command(Game)));
        }
    }
}