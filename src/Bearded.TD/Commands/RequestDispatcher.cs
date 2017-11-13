using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;

namespace Bearded.TD.Commands
{
    interface IRequestDispatcher<out TContext, out TSender>
    {
        void Dispatch(IRequest<TContext, TSender> request);
    }

    class ClientRequestDispatcher<TContext, TSender> : IRequestDispatcher<TContext, TSender>
    {
        private readonly ClientNetworkInterface network;
        private readonly Logger logger;

        public ClientRequestDispatcher(ClientNetworkInterface network, Logger logger)
        {
            this.network = network;
            this.logger = logger;
        }

        public void Dispatch(IRequest<TContext, TSender> request)
        {
            sendToServer(request);
        }

        private void sendToServer(IRequest<TContext, TSender> request)
        {
            var message = network.CreateMessage();

            var serializer = request.Serializer;
            var serializers = Serializers<TContext, TSender>.Instance;
            var id = serializers.RequestId(request.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));

            network.SendMessage(message, NetworkChannel.Chat);
        }
    }

    class ServerRequestDispatcher<TContext, TSender> : IRequestDispatcher<TContext, TSender>
    {
        private readonly ICommandDispatcher<TContext> commandDispatcher;
        private readonly Logger logger;

        public ServerRequestDispatcher(ICommandDispatcher<TContext> commandDispatcher, Logger logger)
        {
            this.commandDispatcher = commandDispatcher;
            this.logger = logger;
        }

        public void Dispatch(IRequest<TContext, TSender> request)
        {
            var command = request.CheckPreconditions()
                ? execute(request)
                : cancel(request);

            commandDispatcher.Dispatch(command);
        }

        private ICommand<TContext> cancel(IRequest<TContext, TSender> request)
        {
            return null;
        }

        private ICommand<TContext> execute(IRequest<TContext, TSender> request)
        {
            return request.ToCommand();
        }
    }
}