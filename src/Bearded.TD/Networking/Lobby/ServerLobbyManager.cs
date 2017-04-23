using System;
using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities;
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
                    case NetIncomingMessageType.StatusChanged:
                        handleStatusChange(msg);
                        break;
                }
            }
        }

        private void handleStatusChange(NetIncomingMessage msg)
        {
            switch (msg.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    var clientInfo = ClientInfo.FromBuffer(msg.SenderConnection.RemoteHailMessage);
                    // Do some checks with the client info.
                    var newPlayer = new Player(new Utilities.Id<Player>(), clientInfo.PlayerName, Color.Black);
                    players.Add(new LobbyPlayer(newPlayer));
                    networkInterface.AddPlayerConnection(newPlayer, msg.SenderConnection);
                    // Send connection accepted/rejected command with current lobby info.
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