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
        private readonly List<LobbyPlayer> players;
        public override IReadOnlyList<LobbyPlayer> Players => players.AsReadOnly();

        public ServerLobbyManager(Logger logger) : base(logger, createDispatchers())
        {
            players = new List<LobbyPlayer> { new LobbyPlayer(Game.Me) };
            players[0].State = LobbyPlayerState.Waiting;
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
            LobbyPlayer lobbyPlayer;
            players.Add(lobbyPlayer = new LobbyPlayer(newPlayer));
            lobbyPlayer.State = LobbyPlayerState.Connecting;
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
            Player player;
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    player = networkInterface.GetSender(msg);
                    var lobbyPlayer = getLobbyPlayer(player);
                    lobbyPlayer.State = LobbyPlayerState.Waiting;
                    break;
                case NetConnectionStatus.Disconnected:
                    player = networkInterface.GetSender(msg);
                    players.RemoveAll(lp => lp.Player == player);
                    networkInterface.RemovePlayerConnection(msg.SenderConnection);
                    break;
            }
        }

        public override void ToggleReadyState()
        {
            var lobbyPlayer = getLobbyPlayer(Game.Me);
            setReadyStateForPlayer(lobbyPlayer,
                lobbyPlayer.State == LobbyPlayerState.Ready ? LobbyPlayerState.Waiting : LobbyPlayerState.Ready);
        }

        private void setReadyStateForPlayer(LobbyPlayer player, LobbyPlayerState lobbyPlayerState)
        {
            player.State = lobbyPlayerState;

            if (Players.All(lobbyPlayer => lobbyPlayer.State == LobbyPlayerState.Ready))
                gameStarted = true;
        }

        private LobbyPlayer getLobbyPlayer(Player player)
        {
            return Players.First(lobbyPlayer => lobbyPlayer.Player == player) ?? throw new ArgumentException();
        }
    }
}