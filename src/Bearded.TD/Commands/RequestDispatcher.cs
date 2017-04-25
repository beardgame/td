using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface IRequestDispatcher
    {
        void Dispatch(IRequest request);
    }

    class ClientRequestDispatcher : IRequestDispatcher
    {
        private readonly ClientNetworkInterface network;

        public ClientRequestDispatcher(ClientNetworkInterface network)
        {
            this.network = network;
        }

        public void Dispatch(IRequest request)
        {
            sendToServer(request);
        }

        private void sendToServer(IRequest request)
        {
            var message = network.CreateMessage();

            var serializer = request.Serializer;
            var serializers = Serializers.Instance;
            var id = serializers.RequestId(request.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));

            network.SendMessage(message, NetworkChannel.Chat);
        }
    }

    class ServerRequestDispatcher : IRequestDispatcher
    {
        private readonly ICommandDispatcher commandDispatcher;

        public ServerRequestDispatcher(ICommandDispatcher commandDispatcher)
        {
            this.commandDispatcher = commandDispatcher;
        }

        public void Dispatch(IRequest request)
        {
            var command = request.CheckPreconditions()
                ? execute(request)
                : cancel(request);

            commandDispatcher.Dispatch(command);
        }

        private ICommand cancel(IRequest request)
        {
            return null;
        }

        private ICommand execute(IRequest request)
        {
            return request.ToCommand();
        }
    }
}