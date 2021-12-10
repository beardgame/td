using Bearded.TD.Networking;

namespace Bearded.TD.Commands;

interface ICommandDispatcher<TObject>
{
    void Dispatch(ISerializableCommand<TObject>? command);
}

sealed class ClientCommandDispatcher<TObject> : ICommandDispatcher<TObject>
{
    private readonly ICommandExecutor executor;

    public ClientCommandDispatcher(ICommandExecutor executor)
    {
        this.executor = executor;
    }

    public void Dispatch(ISerializableCommand<TObject>? command)
    {
        executor.Execute(command!);
    }
}

sealed class ServerCommandDispatcher<TActor, TObject> : ICommandDispatcher<TObject>
{
    private readonly ICommandExecutor executor;
    private readonly ServerNetworkInterface network;

    public ServerCommandDispatcher(ICommandExecutor executor, ServerNetworkInterface network)
    {
        this.executor = executor;
        this.network = network;
    }

    public void Dispatch(ISerializableCommand<TObject>? command)
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
        CommandToNetworkMessageWriter.WriteCommandToMessage<TActor, TObject>(command, message);
        network.SendMessageToAll(message, NetworkChannel.Chat);
    }
}