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

            RegisterIncomingMessageHandler(NetIncomingMessageType.StatusChanged, handleStatusChange);
        }

        protected override NetIncomingMessage GetNextMessage()
        {
            return client.ReadMessage();
        }

        public override void SendMessage(NetOutgoingMessage message, NetworkChannel channel)
        {
            client.SendMessage(message, NetDeliveryMethod.ReliableOrdered, (int) channel);
        }

        private void handleStatusChange(NetIncomingMessage message)
        {
            switch (message.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    Logger.Debug.Log("Server connected :)");
                    break;
                case NetConnectionStatus.Disconnected:
                    Logger.Debug.Log("Server disconnected :(");
                    break;
                default:
                    Logger.Trace.Log("Unhandled status change of type {0}", message.SenderConnection.Status);
                    break;
            }
        }
    }
}