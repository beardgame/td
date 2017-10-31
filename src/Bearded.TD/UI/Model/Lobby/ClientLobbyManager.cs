using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.UI.Model.Loading;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.UI.Model.Lobby
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
            foreach (var msg in networkInterface.GetMessages())
            {
                if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    Game.DataMessageHandler.HandleIncomingMessage(msg);
                }
            }
        }

        public override LoadingManager GetLoadingManager()
        {
            return new ClientLoadingManager(Game, Dispatcher, networkInterface, Logger);
        }
    }
}