using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities;

namespace Bearded.TD.Networking.Lobby
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;
        private readonly Player player;
        public override bool GameStarted { get; }
        public override IReadOnlyList<LobbyPlayer> Players { get; }

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger) : base(logger)
        {
            this.networkInterface = networkInterface;
            this.player = player;
        }

        public override void Update(UpdateEventArgs args)
        {
            // Read lobby messages the server sends us:
            // * Player added/removed
            // * Player ready status changed (including ours)
            // * Game settings changed
            // * Game started
            // * Chat messages
            throw new System.NotImplementedException();
        }

        public override GameInstance BuildInstance(InputManager inputManager)
        {
            var requestDispatcher = new ClientRequestDispatcher();
            var dispatcher = new ClientDispatcher();

            return BuildInstance(player, requestDispatcher, dispatcher, inputManager);
        }
        
        public override void ToggleReadyState()
        {
            // Ask server to change our state
            throw new System.NotImplementedException();
        }
    }
}