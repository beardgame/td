using System.Collections.Generic;
using Bearded.TD.Game.Players;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ServerNetworkInterface : NetworkInterface
    {
        private readonly NetServer server;
        private readonly List<NetConnection> connectedPeers = new List<NetConnection>();

        private readonly Dictionary<NetConnection, Player> connectionToPlayer = new Dictionary<NetConnection, Player>();
        private readonly Dictionary<Player, NetConnection> playerToConnection = new Dictionary<Player, NetConnection>();

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

        public void SendMessageToPlayer(Player player, NetOutgoingMessage message, NetworkChannel channel)
        {
            server.SendMessage(message, playerToConnection[player], NetDeliveryMethod.ReliableOrdered, (int) channel);
        }

        public void AddPlayerConnection(Player player, NetConnection connection)
        {
            connectedPeers.Add(connection);
            connectionToPlayer.Add(connection, player);
            playerToConnection.Add(player, connection);
        }

        public Player GetSender(NetIncomingMessage msg)
        {
            return connectionToPlayer[msg.SenderConnection];
        }

        public void RemovePlayerConnection(Player player)
        {
            var conn = playerToConnection[player];
            connectedPeers.Remove(conn);
            connectionToPlayer.Remove(conn);
            playerToConnection.Remove(player);
        }

        public void RemovePlayerConnection(NetConnection conn)
        {
            var player = connectionToPlayer[conn];
            connectedPeers.Remove(conn);
            connectionToPlayer.Remove(conn);
            playerToConnection.Remove(player);
        }
    }
}