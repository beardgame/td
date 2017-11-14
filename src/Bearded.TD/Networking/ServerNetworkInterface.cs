using System.Collections.Generic;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ServerNetworkInterface<TPeer> : NetworkInterface
    {
        private readonly NetServer server;
        private readonly List<NetConnection> connectedPeers = new List<NetConnection>();

        private readonly Dictionary<NetConnection, TPeer> connectionToPeer = new Dictionary<NetConnection, TPeer>();
        private readonly Dictionary<TPeer, NetConnection> peerToConnection = new Dictionary<TPeer, NetConnection>();

        public int PeerCount => connectedPeers.Count;

        public ServerNetworkInterface(Logger logger) : base(logger)
        {
            var config = new NetPeerConfiguration(Constants.Network.ApplicationName)
            {
                Port = Constants.Network.DefaultPort
            };
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            server = new NetServer(config);
            server.Start();
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

        public void SendMessageToPeer(TPeer player, NetOutgoingMessage message, NetworkChannel channel)
        {
            server.SendMessage(message, peerToConnection[player], NetDeliveryMethod.ReliableOrdered, (int) channel);
        }

        public void AddPeerConnection(TPeer player, NetConnection connection)
        {
            connectedPeers.Add(connection);
            connectionToPeer.Add(connection, player);
            peerToConnection.Add(player, connection);
        }

        public TPeer GetSender(NetIncomingMessage msg)
        {
            return connectionToPeer[msg.SenderConnection];
        }

        public void RemovePeerConnection(TPeer player)
        {
            var conn = peerToConnection[player];
            connectedPeers.Remove(conn);
            connectionToPeer.Remove(conn);
            peerToConnection.Remove(player);
        }

        public void RemovePeerConnection(NetConnection conn)
        {
            var player = connectionToPeer[conn];
            connectedPeers.Remove(conn);
            connectionToPeer.Remove(conn);
            peerToConnection.Remove(player);
        }
    }
}
