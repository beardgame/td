using Bearded.TD.Commands;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    interface IDataMessageHandler<TContext, TSender>
    {
        void HandleIncomingMessage(NetIncomingMessage msg);
    }

    class ServerDataMessageHandler<TContext, TSender> : IDataMessageHandler<TContext, TSender>
    {
        private readonly TContext context;
        private readonly IRequestDispatcher<TContext, TSender> requestDispatcher;
        private readonly ServerNetworkInterface<TSender> networkInterface;
        private readonly Logger logger;

        public ServerDataMessageHandler(
            TContext context, IRequestDispatcher<TContext, TSender> requestDispatcher, ServerNetworkInterface<TSender> networkInterface, Logger logger)
        {
            this.context = context;
            this.requestDispatcher = requestDispatcher;
            this.networkInterface = networkInterface;
            this.logger = logger;
        }

        public void HandleIncomingMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept requests. We should not be receiving commands on the server.
            if (Serializers<TContext, TSender>.Instance.IsRequestSerializer(typeId))
            {
                requestDispatcher.Dispatch(
                    Serializers<TContext, TSender>.Instance.RequestSerializer(typeId).Read<TContext, TSender>(
                        new NetBufferReader(msg), context, networkInterface.GetSender(msg)));
                return;
            }

            logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }
    }

    class ClientDataMessageHandler<TContext, TSender> : IDataMessageHandler<TContext, TSender>
    {
        private readonly TContext context;
        private readonly Logger logger;
        private readonly ICommandDispatcher<TContext> commandDispatcher;

        public ClientDataMessageHandler(TContext context, Logger logger)
        {
            this.context = context;
            this.logger = logger;
            commandDispatcher = new ClientCommandDispatcher<TContext>(new DefaultCommandExecutor<TContext>());
        }

        public void HandleIncomingMessage(NetIncomingMessage msg)
        {
            var typeId = msg.ReadInt32();
            // We only accept commands. We should not be receiving requests on the client.
            if (Serializers<TContext, TSender>.Instance.IsCommandSerializer(typeId))
            {
                commandDispatcher.Dispatch(
                    Serializers<TContext, TSender>.Instance.CommandSerializer(typeId).Read(new NetBufferReader(msg), context));
                return;
            }

            logger.Error.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
        }
    }
}