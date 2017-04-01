using System;
using System.Collections.Generic;
using Bearded.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    abstract class NetworkInterface
    {
        private readonly Dictionary<NetIncomingMessageType, Action<NetIncomingMessage>> incomingMessageHandlers =
                new Dictionary<NetIncomingMessageType, Action<NetIncomingMessage>>();

        protected Logger Logger { get; }

        protected NetworkInterface(Logger logger)
        {
            Logger = logger;

            RegisterIncomingMessageHandler(NetIncomingMessageType.DebugMessage, message => Logger.Debug.Log("Network debug: {0}", message.ReadString()));
        }

        public IEnumerable<NetIncomingMessage> ProcessMessages()
        {
            NetIncomingMessage message;
            while ((message = GetNextMessage()) != null)
            {
                if (message.MessageType == NetIncomingMessageType.Data)
                {
                    yield return message;
                    continue;
                }

                incomingMessageHandlers.TryGetValue(message.MessageType, out Action<NetIncomingMessage> handler);

                if (handler != null)
                    handler(message);
                else
                    Logger.Trace.Log("Unhandled network message: {0}", message.ReadString());
                handler?.Invoke(message);
            }
        }

        protected void RegisterIncomingMessageHandler(
            NetIncomingMessageType incomingMessageType,
            Action<NetIncomingMessage> handler)
        {
            if (incomingMessageHandlers.ContainsKey(incomingMessageType))
                throw new Exception("Cannot register duplicate handlers.");

            incomingMessageHandlers.Add(incomingMessageType, handler);
        }

        protected abstract NetIncomingMessage GetNextMessage();
        public abstract void SendMessage(NetOutgoingMessage message, NetworkChannel channel);
    }
}