using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    class ServerLobbyManager : LobbyManager
    {
        private bool gameStarted;
        private readonly Player player;
        private readonly ServerNetworkInterface networkInterface;

        public override bool GameStarted => gameStarted;
        private readonly List<LobbyPlayer> players;
        public override IReadOnlyList<LobbyPlayer> Players => players.AsReadOnly();

        public ServerLobbyManager(Logger logger) : base(logger)
        {
            player = new Player(new Utilities.Id<Player>(), "The host", Color.Gray);
            players = new List<LobbyPlayer> { new LobbyPlayer(player) };
            networkInterface = new ServerNetworkInterface(logger);
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
            }
            var newPlayer = new Player(new Utilities.Id<Player>(), clientInfo.PlayerName, Color.Black);
            players.Add(new LobbyPlayer(newPlayer));
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
            // Write some information to the message.
            connection.Approve(msg);
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    // Broadcast "new player" command.
                    break;
                case NetConnectionStatus.Disconnected:
                    var player = networkInterface.GetSender(msg);
                    players.RemoveAll(lobbyPlayer => lobbyPlayer.Player == player);
                    networkInterface.RemovePlayerConnection(msg.SenderConnection);
                    break;
            }
        }

        public override void ToggleReadyState()
        {
            var lobbyPlayer = getLobbyPlayer(player);
            setReadyStateForPlayer(lobbyPlayer, !lobbyPlayer.IsReady);
        }

        public override GameInstance BuildInstance(InputManager inputManager)
        {
            var commandDispatcher = new ServerCommandDispatcher(new DefaultCommandExecutor());
            var requestDispatcher = new ServerRequestDispatcher(commandDispatcher);
            var dispatcher = new ServerDispatcher(commandDispatcher);

            return BuildInstance(player, requestDispatcher, dispatcher, inputManager);
        }

        private void setReadyStateForPlayer(LobbyPlayer player, bool isReady)
        {
            player.IsReady = isReady;

            if (Players.All(lobbyPlayer => lobbyPlayer.IsReady))
                gameStarted = true;
        }

        private LobbyPlayer getLobbyPlayer(Player player)
        {
            return Players.First(lobbyPlayer => lobbyPlayer.Player == player) ?? throw new ArgumentException();
        }
    }
}