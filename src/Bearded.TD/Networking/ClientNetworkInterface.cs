﻿using Bearded.TD.Networking.Serialization;
using Bearded.TD.UI.Model.Lobby;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ClientNetworkInterface : NetworkInterface
    {
        private readonly NetClient client;

        public ClientNetworkInterface(Logger logger, string host, ClientInfo clientInfo) : base(logger)
        {
            var config = new NetPeerConfiguration(Constants.Network.ApplicationName);
            client = new NetClient(config);
            client.Start();
            client.Connect(host, Constants.Network.DefaultPort, createHailMessage(clientInfo));
        }

        public override NetOutgoingMessage CreateMessage()
        {
            return client.CreateMessage();
        }

        public override void Shutdown()
        {
            client.Shutdown("I don't hate you.");
        }

        protected override NetIncomingMessage GetNextMessage()
        {
            return client.ReadMessage();
        }

        public void SendMessage(NetOutgoingMessage message, NetworkChannel channel)
        {
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, (int) channel);
        }

        private NetOutgoingMessage createHailMessage(ClientInfo clientInfo)
        {
            var msg = client.CreateMessage();
            clientInfo.Serialize(new NetBufferWriter(msg));
            return msg;
        }
    }
}