using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.Synchronization;
using Bearded.TD.Networking.Loading;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger)
            : base(logger, player,
                  (new ClientRequestDispatcher(networkInterface, logger), new ClientDispatcher(), new ClientGameSynchronizer()),
                  game => createDataMessageHandler(game, logger))
        {
            this.networkInterface = networkInterface;
        }

        private static IDataMessageHandler createDataMessageHandler(GameInstance game, Logger logger)
        {
            return new ClientDataMessageHandler(game, logger);
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