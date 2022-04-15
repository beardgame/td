using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer;

sealed class ServerMasterServer : MasterServer
{
    public ServerMasterServer(NetServer server) : base(server) { }

    public bool RegisterLobby(Proto.Lobby lobbyInfo)
    {
        if (!IsMasterServerResolved) return false;

        var request = new Proto.RegisterLobbyRequest
        {
            Lobby = lobbyInfo,
            Address = ByteString.CopyFrom(NetUtility.GetMyAddress(out _).GetAddressBytes()),
            Port = Constants.Network.DefaultPort
        };
        var protoMsg = CreateMessage();
        protoMsg.RegisterLobby = request;
        SendMessage(protoMsg);
        return true;
    }
}
