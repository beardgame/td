using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    class ClientNetworkInterface : NetworkInterface
    {
        private readonly NetClient client;

        public ClientNetworkInterface(Logger logger, string host) : base(logger)
        {
            var config = new NetPeerConfiguration(Constants.Network.ApplicationName);
            client = new NetClient(config);
            client.Start();
            client.Connect(host, Constants.Network.DefaultPort);
        }

        protected override NetIncomingMessage GetNextMessage()
        {
            return client.ReadMessage();
        }

        public void SendMessage(NetOutgoingMessage message, NetworkChannel channel)
        {
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, (int) channel);
        }
    }
}