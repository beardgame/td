using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands;

interface IRequestDispatcher<TActor, TObject>
{
    void Dispatch(TActor actor, IRequest<TActor, TObject> request);
}

sealed class ClientRequestDispatcher<TActor, TObject> : IRequestDispatcher<TActor, TObject>
{
    private readonly ClientNetworkInterface network;

    public ClientRequestDispatcher(ClientNetworkInterface network)
    {
        this.network = network;
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

sealed class ServerRequestDispatcher<TActor, TObject> : IRequestDispatcher<TActor, TObject>
{
    private readonly ICommandDispatcher<TObject> commandDispatcher;

    public ServerRequestDispatcher(ICommandDispatcher<TObject> commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
    }

    public void Dispatch(TActor actor, IRequest<TActor, TObject> request)
    {
        var command = request.CheckPreconditions(actor)
            ? execute(request)
            : cancel();

        commandDispatcher.Dispatch(command);
    }

    private static ISerializableCommand<TObject>? cancel()
    {
        return null;
    }

    private ISerializableCommand<TObject>? execute(IRequest<TActor, TObject> request)
    {
        return request.ToCommand();
    }
}