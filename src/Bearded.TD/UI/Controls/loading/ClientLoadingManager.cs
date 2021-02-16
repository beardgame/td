using Bearded.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.General;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;

namespace Bearded.TD.UI.Controls
{
    class ClientLoadingManager : LoadingManager
    {
        public ClientLoadingManager(GameInstance game, NetworkInterface networkInterface)
            : base(game, networkInterface) { }

        public override void Update(UpdateEventArgs args)
        {
            base.Update(args);

            // Instantly finish loading for now.
            if (Game.Me.ConnectionState == PlayerConnectionState.ProcessingLoadingData)
            {
                Game.Request(ChangePlayerState.Request(Game.Me, PlayerConnectionState.FinishedLoading));
            }
        }
    }
}
