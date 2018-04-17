using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game;
using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Mods;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.TD.UI.Model.Loading;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.UI.Model.Lobby
{
    class ServerLobbyManager : LobbyManager
    {
        private const float heartbeatTimeSeconds = 10;

        private readonly ServerNetworkInterface networkInterface;
        private float secondsUntilNextHeartbeat;

        public ServerLobbyManager(ServerNetworkInterface networkInterface, Logger logger, ContentManager contentManager)
            : base(new ServerGameContext(networkInterface, logger), contentManager)
        {
            this.networkInterface = networkInterface;

            // First heartbeat automatically registers lobby.
            doHeartbeat();
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
                        Game.DataMessageHandler.HandleIncomingMessage(msg);
                        break;
                }
            }

            if (Game.Players.All(p => p.ConnectionState == PlayerConnectionState.Ready))
            {
                Dispatcher.RunOnlyOnServer(
                    commandDispatcher => commandDispatcher.Dispatch(BeginLoadingGame.Command(Game)));
            }

            secondsUntilNextHeartbeat -= args.ElapsedTimeInSf;
            if (secondsUntilNextHeartbeat <= 0)
            {
                doHeartbeat();
            }
        }

        private void doHeartbeat()
        {
            networkInterface.Master.RegisterLobby(
                new Proto.Lobby
                {
                    Id = networkInterface.UniqueIdentifier,
                    Name = $"{Game.Me.Name}'s game",
                    MaxNumPlayers = 4,
                    CurrentNumPlayers = 1,
                }
            );
            secondsUntilNextHeartbeat = heartbeatTimeSeconds;
        }

        private void handleConnectionApproval(NetIncomingMessage msg)
        {
            var clientInfo = ClientInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
            if (!checkClientInfo(clientInfo, out string rejectionReason))
            {
                msg.SenderConnection.Deny(rejectionReason);
                return;
            }
            var newPlayer = new Player(Game.Ids.GetNext<Player>(), clientInfo.PlayerName);
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
                    foreach (var p in Game.Players)
                    {
                        if (p == networkInterface.GetSender(msg)) continue;

                        // For now we manually send this event to just the one player, but we should make an interface for this.
                        var outMsg = networkInterface.CreateMessage();
                        var serializer = AddPlayer.Command(Game, p).Serializer;
                        outMsg.Write(Serializers<GameInstance>.Instance.CommandId(serializer));
                        serializer.Serialize(new NetBufferWriter(outMsg));
                        networkInterface.SendMessageToPlayer(networkInterface.GetSender(msg), outMsg, NetworkChannel.Chat);
                    }
                    break;
                case NetConnectionStatus.Disconnected:
                    Game.RemovePlayer(networkInterface.GetSender(msg));
                    networkInterface.RemovePlayerConnection(msg.SenderConnection);
                    break;
            }
        }

        public override LoadingManager GetLoadingManager()
        {
            return new ServerLoadingManager(Game, Dispatcher, networkInterface, Logger);
        }
    }
}