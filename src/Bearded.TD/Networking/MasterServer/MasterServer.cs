using System.Linq;
using System.Net;
using System.Net.Sockets;
using Bearded.TD.Meta;
using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer;

abstract class MasterServer
{
    private readonly NetPeer peer;
    private readonly IPEndPoint? masterServerEndPoint;

    protected bool IsMasterServerResolved => masterServerEndPoint != null;

    protected MasterServer(NetPeer peer)
    {
        this.peer = peer;
        var v4ip = Dns
            .GetHostAddresses(UserSettings.Instance.Misc.MasterServerAddress, AddressFamily.InterNetwork)
            .FirstOrDefault();
        masterServerEndPoint = new IPEndPoint(v4ip, Constants.Network.MasterServerPort);
    }

    protected Proto.MasterServerMessage CreateMessage()
    {
        return new Proto.MasterServerMessage
        {
            GameInfo = gameInfo
        };
    }

    protected void SendMessage(Proto.MasterServerMessage protoMsg)
    {
        var msg = peer.CreateMessage();
        msg.Write(protoMsg.ToByteArray());
        peer.SendUnconnectedMessage(msg, masterServerEndPoint);
    }

    private Proto.GameInfo gameInfo => new();
}
