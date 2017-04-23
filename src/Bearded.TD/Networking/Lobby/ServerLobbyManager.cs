using System;
using System.Collections.Generic;
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
        private readonly List<Player> players;
        public override IReadOnlyList<Player> Players => players.AsReadOnly();

        public ServerLobbyManager(Logger logger) : base(logger, createDispatchers())
        {
            players = new List<Player> { Game.Me };
            players[0].ConnectionState = PlayerConnectionState.Waiting;
            networkInterface = new ServerNetworkInterface(logger);
        }

        private static (IRequestDispatcher, IDispatcher) createDispatchers()
        {
            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor());
            var requestDispatcher = new ServerRequestDispatcher(commandDispatcher);
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
            var newPlayer = new Player(Game.Ids.GetNext<Player>(), clientInfo.PlayerName, Color.Black);
            players.Add(newPlayer);
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
                    players.Remove(networkInterface.GetSender(msg));
                    networkInterface.RemovePlayerConnection(msg.SenderConnection);
                    break;
            }
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

            if (Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
                gameStarted = true;
        }
    }
}