using Bearded.TD.Networking.MasterServer;
using Lidgren.Network;

namespace Bearded.TD.Networking;

sealed class ServerNetworkInterface : NetworkInterface
{
    private readonly NetServer server;

    public ServerMasterServer Master { get; }

    public override long UniqueIdentifier => server.UniqueIdentifier;

    public ServerNetworkInterface()
    {
        var config = new NetPeerConfiguration(Constants.Network.ApplicationName)
        {
            Port = Constants.Network.DefaultPort
        };
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
        server = new NetServer(config);
        server.Start();

        Master = new ServerMasterServer(server);
    }

    public override void Shutdown()
    {
        server.Shutdown("I don't hate you.");
    }

    public override NetOutgoingMessage CreateMessage()
    {
        return server.CreateMessage();
    }

    protected override NetIncomingMessage GetNextMessage()
    {
        return server.ReadMessage();
    }

    public void SendMessageToAll(NetOutgoingMessage message, NetworkChannel channel)
    {
        server.SendToAll(message, null, NetDeliveryMethod.ReliableOrdered, (int) channel);
    }

    public void SendMessageToConnection(NetConnection conn, NetOutgoingMessage message, NetworkChannel channel)
    {
        server.SendMessage(message, conn, NetDeliveryMethod.ReliableOrdered, (int) channel);
    }
}