using amulware.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Lobby
{
    class ClientLobbyManager : LobbyManager
    {
        private readonly ClientNetworkInterface networkInterface;
        private readonly ClientCommandDispatcher commandDispatcher;
        public override bool GameStarted { get; }

        public ClientLobbyManager(ClientNetworkInterface networkInterface, Player player, Logger logger)
            : base(logger, player, (new ClientRequestDispatcher(networkInterface, logger), new ClientDispatcher()))
        {
            this.networkInterface = networkInterface;
            commandDispatcher = new ClientCommandDispatcher(new DefaultCommandExecutor());
        }

        public override void Update(UpdateEventArgs args)
        {
            foreach (var msg in networkInterface.GetMessages())
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        handleIncomingDataMessage(msg);
                        break;
                }
            }
        }

        private void handleIncomingDataMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept commands. We should not be receiving requests on the client.
            if (Serializers.Instance.IsCommandSerializer(typeId))
            {
                commandDispatcher.Dispatch(
                    Serializers.Instance.CommandSerializer(typeId).Read(new NetBufferReader(msg), Game));
                return;
            }

            Logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }

        public override void ToggleReadyState()
        {
            // Ask server to change our state
        }
    }
}