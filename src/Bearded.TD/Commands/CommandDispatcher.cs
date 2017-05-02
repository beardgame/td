
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
    }

    class ClientCommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandExecutor executor;

        public ClientCommandDispatcher(ICommandExecutor executor)
        {
            this.executor = executor;
        }

        public void Dispatch(ICommand command)
        {
            executor.Execute(command);
        }
    }

    class ServerCommandDispatcher : ICommandDispatcher
    {
        private readonly ICommandExecutor executor;
        private readonly ServerNetworkInterface network;

        public ServerCommandDispatcher(ICommandExecutor executor, ServerNetworkInterface network)
        {
            this.executor = executor;
            this.network = network;
        }

        public void Dispatch(ICommand command)
        {
            if (command == null)
                return;

            sendToAllPlayers(command);

            executor.Execute(command);
        }

        private void sendToAllPlayers(ICommand command)
        {
            var message = network.CreateMessage();

            var serializer = command.Serializer;
            var serializers = Serializers.Instance;
            var id = serializers.CommandId(command.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));

            network.SendMessageToAll(message, NetworkChannel.Chat);
        }
    }
}