using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Networking;
using Bearded.TD.Utilities;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Model.Loading
{
    class ClientLoadingManager : LoadingManager
    {
        public ClientLoadingManager(
            GameInstance game, IDispatcher dispatcher, NetworkInterface networkInterface, Logger logger)
            : base(game, dispatcher, networkInterface, logger) { }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            if (Game.Me.ConnectionState == PlayerConnectionState.DownloadingMods)
            {
                if (!HasModsQueuedForLoading)
                    Game.ContentManager.Mods.ForEach(LoadMod);
                else if (HasModsQueuedForLoading && HaveAllModsFinishedLoading)
                    Game.RequestDispatcher.Dispatch(
                        ChangePlayerState.Request(Game.Me, PlayerConnectionState.AwaitingLoadingData));
            }

            // Also just instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
            {
                Game.RequestDispatcher.Dispatch(
                    ChangePlayerState.Request(Game.Me, PlayerConnectionState.FinishedLoading));
            }
        }
    }
}
