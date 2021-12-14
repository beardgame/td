using Bearded.TD.Commands;
using Bearded.TD.Commands.Serialization;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Game;

abstract class DataMessageHandler : INetworkMessageHandler
{
    public bool Accepts(NetIncomingMessage message)
        => message.MessageType == NetIncomingMessageType.Data;

    public abstract void Handle(NetIncomingMessage message);
}

class ServerDataMessageHandler : DataMessageHandler
{
    private readonly GameInstance game;
    private readonly Logger logger;

    public ServerDataMessageHandler(GameInstance game, Logger logger)
    {
        this.game = game;
        this.logger = logger;
    }

    public override void Handle(NetIncomingMessage msg)
    {
        var typeId = msg.ReadInt32();
        // We only accept requests. We should not be receiving commands on the server.
        if (Serializers<Player, GameInstance>.Instance.IsRequestSerializer(typeId))
        {
            game.RequestDispatcher.Dispatch(
                game.PlayerFor(msg),
                Serializers<Player, GameInstance>
                    .Instance.RequestSerializer(typeId).Read(new NetBufferReader(msg), game));
            return;
        }

        logger.Error?.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
    }
}

class ClientDataMessageHandler : DataMessageHandler
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

    public override void Handle(NetIncomingMessage msg)
    {
        var typeId = msg.ReadInt32();
        // We only accept commands. We should not be receiving requests on the client.
        if (Serializers<Player, GameInstance>.Instance.IsCommandSerializer(typeId))
        {
            commandDispatcher.Dispatch(
                Serializers<Player,GameInstance>
                    .Instance.CommandSerializer(typeId).Read(new NetBufferReader(msg), game));
            return;
        }

        logger.Error?.Log($"We received a data message with type {typeId}, which is not a valid request ID.");
    }
}