using System.Collections.Generic;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ServerNetworkInterface : NetworkInterface
    {
        private readonly NetServer server;
        private readonly List<NetConnection> connectedPeers = new List<NetConnection>();

        public ServerNetworkInterface(Logger logger) : base(logger)
        {
            var config = new NetPeerConfiguration(Constants.Network.ApplicationName)
            {
                Port = Constants.Network.DefaultPort
            };
            server = new NetServer(config);
            server.Start();

            RegisterIncomingMessageHandler(NetIncomingMessageType.StatusChanged, handleStatusChange);
        }

        protected override NetIncomingMessage GetNextMessage()
        {
            return server.ReadMessage();
        }

        public override void SendMessage(NetOutgoingMessage message, NetworkChannel channel)
        {
            server.SendToAll(message, null, NetDeliveryMethod.ReliableOrdered, (int) channel);
        }

        private void handleStatusChange(NetIncomingMessage message)
        {
            switch (message.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    Logger.Debug.Log("Somebody connected :)");
                    connectedPeers.Add(message.SenderConnection);
                    break;
                case NetConnectionStatus.Disconnected:
                    Logger.Debug.Log("Somebody disconnected :(");
                    connectedPeers.Remove(message.SenderConnection);
                    break;
                default:
                    Logger.Trace.Log("Unhandled status change of type {0}", message.SenderConnection.Status);
                    break;
            }
        }
    }
}