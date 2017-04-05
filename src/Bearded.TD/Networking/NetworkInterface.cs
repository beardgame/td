using System.Collections.Generic;
using Bearded.Utilities;
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

        public IEnumerable<NetIncomingMessage> GetMessages()
        {
            NetIncomingMessage message;
            while ((message = GetNextMessage()) != null)
            {
                if (message.MessageType == NetIncomingMessageType.DebugMessage)
                {
                    Logger.Debug.Log("Network debug: {0}", message.ReadString());
                    continue;
                }

                yield return message;
            }
        }

        protected abstract NetIncomingMessage GetNextMessage();
    }
}