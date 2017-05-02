using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;
        private readonly IDataMessageHandler dataMessageHandler;

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger)
            : base(logger, player, (new ClientRequestDispatcher(networkInterface, logger), new ClientDispatcher()))
        {
            this.networkInterface = networkInterface;
            dataMessageHandler = new ClientDataMessageHandler(Game, logger);
        }

        public override void Update(UpdateEventArgs args)
        {
            foreach (var msg in networkInterface.GetMessages())
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        dataMessageHandler.HandleIncomingMessage(msg);
                        break;
                }
            }
        }
    }
}