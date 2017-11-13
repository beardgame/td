using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface ICommandDispatcher<out TContext>
    {
        void Dispatch(ICommand<TContext> command);
    }

    class ClientCommandDispatcher<TContext> : ICommandDispatcher<TContext>
    {
        private readonly ICommandExecutor<TContext> executor;

        public ClientCommandDispatcher(ICommandExecutor<TContext> executor)
        {
            this.executor = executor;
        }

        public void Dispatch(ICommand<TContext> command)
        {
            executor.Execute(command);
        }
    }

    class ServerCommandDispatcher<TContext, TPeer> : ICommandDispatcher<TContext>
    {
        private readonly ICommandExecutor<TContext> executor;
        private readonly ServerNetworkInterface<TPeer> network;

        public ServerCommandDispatcher(ICommandExecutor<TContext> executor, ServerNetworkInterface<TPeer> network)
        {
            this.executor = executor;
            this.network = network;
        }

        public void Dispatch(ICommand<TContext> command)
        {
            if (command == null)
                return;

            sendToAllPeers(command);

            executor.Execute(command);
        }

        private void sendToAllPeers(ICommand<TContext> command)
        {
            if (network.PeerCount == 0)
                return;

            var message = network.CreateMessage();

            var serializer = command.Serializer;
            var serializers = Serializers<TContext, TPeer>.Instance;
            var id = serializers.CommandId(command.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));

            network.SendMessageToAll(message, NetworkChannel.Chat);
        }
    }
}
