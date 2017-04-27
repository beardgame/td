using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    class ServerLobbyManager : LobbyManager
    {
        private bool gameStarted;
        private readonly ServerNetworkInterface networkInterface;

        public override bool GameStarted => gameStarted;

        public ServerLobbyManager(ServerNetworkInterface networkInterface, Logger logger)
            : base(logger, createDispatchers(networkInterface, logger))
        {
            this.networkInterface = networkInterface;
        }

        private static (IRequestDispatcher, IDispatcher) createDispatchers(ServerNetworkInterface network, Logger logger)
        {
            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor(), network);
            var requestDispatcher = new ServerRequestDispatcher(commandDispatcher, logger);
            var dispatcher = new ServerDispatcher(commandDispatcher);

            return (requestDispatcher, dispatcher);
        }

        public override void Update(UpdateEventArgs args)
        {
            foreach (var msg in networkInterface.GetMessages())
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        handleConnectionApproval(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        handleStatusChange(msg);
                        break;
                    case NetIncomingMessageType.Data:
                        handleIncomingDataMessage(msg);
                        break;
                }
            }
        }

        private void handleConnectionApproval(NetIncomingMessage msg)
        {
            var clientInfo = ClientInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            if (!checkClientInfo(clientInfo, out string rejectionReason))
            {
                msg.SenderConnection.Deny(rejectionReason);
                return;
            }
            var newPlayer = new Player(Game.Ids.GetNext<Player>(), clientInfo.PlayerName, Color.Blue);
            Game.AddPlayer(newPlayer);
            newPlayer.ConnectionState = PlayerConnectionState.Connecting;
            networkInterface.AddPlayerConnection(newPlayer, msg.SenderConnection);
            sendApproval(newPlayer, msg.SenderConnection);
        }

        private bool checkClientInfo(ClientInfo clientInfo, out string rejectionReason)
        {
            if (clientInfo.PlayerName.Trim() == "")
            {
                rejectionReason = "Empty name is not allowed";
                return false;
            }

            rejectionReason = null;
            return true;
        }

        private void sendApproval(Player player, NetConnection connection)
        {
            var msg = networkInterface.CreateMessage();
            var info = LobbyPlayerInfo.ForPlayer(player);
            info.Serialize(new NetBufferWriter(msg));
            connection.Approve(msg);
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    networkInterface.GetSender(msg).ConnectionState = PlayerConnectionState.Waiting;
                    break;
                case NetConnectionStatus.Disconnected:
                    Game.RemovePlayer(networkInterface.GetSender(msg));
                    networkInterface.RemovePlayerConnection(msg.SenderConnection);
                    break;
            }
        }

        private void handleIncomingDataMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept requests. We should not be receiving commands on the server.
            if (Serializers.Instance.IsRequestSerializer(typeId))
            {
                Game.RequestDispatcher.Dispatch(
                    Serializers.Instance.RequestSerializer(typeId).Read(new NetBufferReader(msg), Game));
                return;
            }

            Logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }

        public override void ToggleReadyState()
        {
            setConnectionStateForPlayer(Game.Me,
                Game.Me.ConnectionState == PlayerConnectionState.Ready
                    ? PlayerConnectionState.Waiting
                    : PlayerConnectionState.Ready);
        }

        private void setConnectionStateForPlayer(Player player, PlayerConnectionState connectionState)
        {
            player.ConnectionState = connectionState;

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
                gameStarted = true;
        }
    }
}