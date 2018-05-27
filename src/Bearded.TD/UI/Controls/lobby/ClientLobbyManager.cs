using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.UI.Model.Loading;
using Bearded.Utilities.IO;

namespace Bearded.TD.UI.Controls
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger, ContentManager contentManager)
            : base(player, new ClientGameContext(networkInterface, logger), contentManager)
        {
            this.networkInterface = networkInterface;
        }

        public override void Update(UpdateEventArgs args)
        {
            networkInterface.ConsumeMessages();
        }

        public override LoadingManager GetLoadingManager()
        {
            return new ClientLoadingManager(Game, Dispatcher, networkInterface, Logger);
        }
    }
}