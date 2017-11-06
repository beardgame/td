using System.Collections.Generic;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    abstract class NetworkInterface
    {
        protected Logger Logger { get; }

        protected NetworkInterface(Logger logger)
        {
            Logger = logger;
        }

        public abstract NetOutgoingMessage CreateMessage();
        public abstract void Shutdown();

        public IEnumerable<NetIncomingMessage> GetMessages()
        {
            NetIncomingMessage message;
            while ((message = GetNextMessage()) != null)
            {
                Logger.Trace.Log($"Incoming message: {message.MessageType} (length: {message.Data.Length})");

                if (message.MessageType == NetIncomingMessageType.DebugMessage)
                {
                    Logger.Debug.Log("Network debug: {0}", message.ReadString());
                    continue;
                }
                if (message.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    Logger.Debug.Log("Network status changed: {0}", message.SenderConnection.Status);
                }

                yield return message;
            }
        }

        protected abstract NetIncomingMessage GetNextMessage();
    }
}