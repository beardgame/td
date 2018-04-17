using Bearded.TD.Commands;
using Bearded.TD.Game;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    interface IDataMessageHandler
    {
        void HandleIncomingMessage(NetIncomingMessage msg);
    }

    class ServerDataMessageHandler : IDataMessageHandler
    {
        private readonly GameInstance game;
        private readonly ServerNetworkInterface networkInterface;
        private readonly Logger logger;

        public ServerDataMessageHandler(GameInstance game, ServerNetworkInterface networkInterface, Logger logger)
        {
            this.game = game;
            this.networkInterface = networkInterface;
            this.logger = logger;
        }

        public void HandleIncomingMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept requests. We should not be receiving commands on the server.
            if (Serializers<GameInstance>.Instance.IsRequestSerializer(typeId))
            {
                game.RequestDispatcher.Dispatch(
                    Serializers<GameInstance>.Instance.RequestSerializer(typeId).Read(new NetBufferReader(msg), game, networkInterface.GetSender(msg)));
                return;
            }

            logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }
    }

    class ClientDataMessageHandler : IDataMessageHandler
    {
        private readonly GameInstance game;
        private readonly Logger logger;
        private readonly ICommandDispatcher<GameInstance> commandDispatcher;

        public ClientDataMessageHandler(GameInstance game, Logger logger)
        {
            this.game = game;
            this.logger = logger;
            commandDispatcher = new ClientCommandDispatcher<GameInstance>(new DefaultCommandExecutor());
        }

        public void HandleIncomingMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept commands. We should not be receiving requests on the client.
            if (Serializers<GameInstance>.Instance.IsCommandSerializer(typeId))
            {
                commandDispatcher.Dispatch(
                    Serializers<GameInstance>.Instance.CommandSerializer(typeId).Read(new NetBufferReader(msg), game));
                return;
            }

            logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }
    }
}