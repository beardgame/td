﻿using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Commands;
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
        private readonly IDataMessageHandler dataMessageHandler;

        public override bool GameStarted => gameStarted;

        public ServerLobbyManager(ServerNetworkInterface networkInterface, Logger logger)
            : base(logger, createDispatchers(networkInterface, logger))
        {
            this.networkInterface = networkInterface;
            dataMessageHandler = new ServerDataMessageHandler(Game, logger);
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
                        dataMessageHandler.HandleIncomingMessage(msg);
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
            Dispatcher.RunOnlyOnServer(
                commandDispatcher => commandDispatcher.Dispatch(AddPlayer.Command(Game, newPlayer)));
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
                    // For now we manually send this event to just the one player, but we should make an interface for this.
                    var outMsg = networkInterface.CreateMessage();
                    var serializer = AddPlayer.Command(Game, Game.Me).Serializer;
                    outMsg.Write(Serializers.Instance.CommandId(serializer));
                    serializer.Serialize(new NetBufferWriter(outMsg));
                    networkInterface.SendMessageToPlayer(networkInterface.GetSender(msg), outMsg, NetworkChannel.Chat);
                    break;
                case NetConnectionStatus.Disconnected:
                    Game.RemovePlayer(networkInterface.GetSender(msg));
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

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
                gameStarted = true;
        }
    }
}