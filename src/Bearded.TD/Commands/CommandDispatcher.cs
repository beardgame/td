using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands
{
    interface ICommandDispatcher<TObject>
    {
        void Dispatch(ISerializableCommand<TObject> command);
    }

    class ClientCommandDispatcher<TObject> : ICommandDispatcher<TObject>
    {
        private readonly ICommandExecutor executor;

        public ClientCommandDispatcher(ICommandExecutor executor)
        {
            this.executor = executor;
        }

        public void Dispatch(ISerializableCommand<TObject> command)
        {
            executor.Execute(command);
        }
    }

    class ServerCommandDispatcher<TObject> : ICommandDispatcher<TObject>
    {
        private readonly ICommandExecutor executor;
        private readonly ServerNetworkInterface network;

        public ServerCommandDispatcher(ICommandExecutor executor, ServerNetworkInterface network)
        {
            this.executor = executor;
            this.network = network;
        }

        public void Dispatch(ISerializableCommand<TObject> command)
        {
            if (command == null)
                return;

            sendToAllPlayers(command);

            executor.Execute(command);
        }

        private void sendToAllPlayers(ISerializableCommand<TObject> command)
        {
            if (network.PeerCount == 0)
                return;

            var message = network.CreateMessage();
            CommandToNetworkMessageWriter.WriteCommandToMessage(command, message);
            network.SendMessageToAll(message, NetworkChannel.Chat);
        }
    }
}
