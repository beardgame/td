using System.Net;
using Bearded.TD.Meta;
using Google.Protobuf;
using Lidgren.Network;

namespace Bearded.TD.Networking.MasterServer
{
    abstract class MasterServer
    {
        private readonly NetPeer peer;
        private readonly IPEndPoint masterServerEndPoint;

        protected MasterServer(NetPeer peer)
        {
            this.peer = peer;
            masterServerEndPoint = NetUtility.Resolve(
                UserSettings.Instance.Misc.MasterServerAddress, Constants.Network.MasterServerPort);
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

        private Proto.GameInfo gameInfo => new Proto.GameInfo();
    }
}
