using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Loading
{
    class ClientLoadingManager : LoadingManager
    {
        public ClientLoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface,
            IDataMessageHandler dataMessageHandler, Logger logger)
            : base(game, dispatcher, networkInterface, dataMessageHandler, logger)
        {
        }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
                Game.RequestDispatcher.Dispatch(
                    ChangePlayerState.Request(Game.Me, PlayerConnectionState.FinishedLoading));
        }
    }
}