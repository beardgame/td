using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;
        public override bool GameStarted { get; }
        private readonly List<Player> players;
        public override IReadOnlyList<Player> Players => players.AsReadOnly();

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger)
            : base(logger, player, (new ClientRequestDispatcher(), new ClientDispatcher()))
        {
            players = new List<Player> { Game.Me };
            players[0].ConnectionState = PlayerConnectionState.Waiting;
            this.networkInterface = networkInterface;
        }

        public override void Update(UpdateEventArgs args)
        {
            // Read lobby messages the server sends us:
            // * Player added/removed
            // * Player ready status changed (including ours)
            // * Game settings changed
            // * Game started
            // * Chat messages
        }
        
        public override void ToggleReadyState()
        {
            // Ask server to change our state
        }
    }
}