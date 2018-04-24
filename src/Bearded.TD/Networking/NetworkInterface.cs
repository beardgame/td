using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    abstract class NetworkInterface
    {
        private readonly List<INetworkMessageHandler> messageHandlers = new List<INetworkMessageHandler>();
        
        private readonly List<NetConnection> connectedPeers = new List<NetConnection>();
        public int PeerCount => connectedPeers.Count;

        public abstract NetOutgoingMessage CreateMessage();
        public abstract void Shutdown();

        public void RegisterMessageHandler(INetworkMessageHandler messageHandler)
        {
            messageHandlers.Add(messageHandler);
        }

        public void UnregisterMessageHandler(INetworkMessageHandler messageHandler)
        {
            messageHandlers.Remove(messageHandler);
        }

        public void ConsumeMessages()
        {
            NetIncomingMessage message;
            while ((message = GetNextMessage()) != null)
            {
                if (message.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    switch (message.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            connectedPeers.Add(message.SenderConnection);
                            break;
                        case NetConnectionStatus.Disconnected:
                            connectedPeers.Remove(message.SenderConnection);
                            break;
                    }
                }

                var acceptedHandlers = messageHandlers.Where(handler => handler.Accepts(message)).ToList();

                foreach (var handler in acceptedHandlers)
                {
                    handler.Handle(message);
                }
            }
        }

        protected abstract NetIncomingMessage GetNextMessage();
    }
}