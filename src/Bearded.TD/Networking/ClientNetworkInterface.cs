using System.Net;
using Bearded.TD.Networking.MasterServer;
using Bearded.TD.Networking.Serialization;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ClientNetworkInterface : NetworkInterface
    {
        private readonly NetClient client;

        public ClientMasterServer Master { get; }

        public ClientNetworkInterface(Logger logger) : base(logger)
        {
			var config = new NetPeerConfiguration(Constants.Network.ApplicationName);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
			client = new NetClient(config);
			client.Start();

			Master = new ClientMasterServer(client);
        }

        public void Connect(string host, ClientInfo clientInfo)
        {
            client.Connect(host, Constants.Network.DefaultPort, createHailMessage(clientInfo));
        }

        public void Connect(IPEndPoint endpoint, ClientInfo clientInfo)
        {
            client.Connect(endpoint, createHailMessage(clientInfo));
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