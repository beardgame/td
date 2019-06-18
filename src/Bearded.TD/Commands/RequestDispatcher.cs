using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;

namespace Bearded.TD.Commands
{
    interface IRequestDispatcher<TActor, TObject>
    {
        void Dispatch(TActor actor, IRequest<TActor, TObject> request);
    }

    class ClientRequestDispatcher<TActor, TObject> : IRequestDispatcher<TActor, TObject>
    {
        private readonly ClientNetworkInterface network;
        private readonly Logger logger;

        public ClientRequestDispatcher(ClientNetworkInterface network, Logger logger)
        {
            this.network = network;
            this.logger = logger;
        }

        public void Dispatch(TActor _, IRequest<TActor, TObject> request)
        {
            sendToServer(request);
        }

        private void sendToServer(IRequest<TActor, TObject> request)
        {
            var message = network.CreateMessage();

            var serializer = request.Serializer;
            var serializers = Serializers<TActor, TObject>.Instance;
            var id = serializers.RequestId(request.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));

            network.SendMessage(message, NetworkChannel.Chat);
        }
    }

    class ServerRequestDispatcher<TActor, TObject> : IRequestDispatcher<TActor, TObject>
    {
        private readonly ICommandDispatcher<TObject> commandDispatcher;
        private readonly Logger logger;

        public ServerRequestDispatcher(ICommandDispatcher<TObject> commandDispatcher, Logger logger)
        {
            this.commandDispatcher = commandDispatcher;
            this.logger = logger;
        }

        public void Dispatch(TActor actor, IRequest<TActor, TObject> request)
        {
            var command = request.CheckPreconditions(actor)
                ? execute(request)
                : cancel(request);

            commandDispatcher.Dispatch(command);
        }

        private ISerializableCommand<TObject> cancel(IRequest<TActor, TObject> request)
        {
            return null;
        }

        private ISerializableCommand<TObject> execute(IRequest<TActor, TObject> request)
        {
            return request.ToCommand();
        }
    }
}
