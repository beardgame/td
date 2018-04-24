using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.Utilities.IO;
using Lidgren.Network;

namespace Bearded.TD.Networking
{
    sealed class NetworkDebugMessageHandler : INetworkMessageHandler
    {
        private static readonly IReadOnlyDictionary<NetIncomingMessageType, Func<Logger, Logger.Writer>> typeToWriter;
        private static readonly ISet<NetIncomingMessageType> acceptedMessageTypes;

        static NetworkDebugMessageHandler()
        {
            typeToWriter =
                new ReadOnlyDictionary<NetIncomingMessageType, Func<Logger, Logger.Writer>>(
                    new Dictionary<NetIncomingMessageType, Func<Logger, Logger.Writer>>
                    {
                        {
                            NetIncomingMessageType.VerboseDebugMessage,
                            logger => logger.Trace
                        },
                        {
                            NetIncomingMessageType.DebugMessage,
                            logger => logger.Debug
                        },
                        {
                            NetIncomingMessageType.WarningMessage,
                            logger => logger.Warning
                        },
                        {
                            NetIncomingMessageType.ErrorMessage,
                            logger => logger.Error
                        }
                    });
            acceptedMessageTypes = new HashSet<NetIncomingMessageType>(typeToWriter.Keys);
        }

        private readonly IReadOnlyDictionary<NetIncomingMessageType, Logger.Writer> writers;

        public NetworkDebugMessageHandler(Logger logger)
        {
            writers = new ReadOnlyDictionary<NetIncomingMessageType, Logger.Writer>(
                    typeToWriter.ToDictionary(pair => pair.Key, pair => pair.Value(logger)));
        }

        public bool Accepts(NetIncomingMessage message) => acceptedMessageTypes.Contains(message.MessageType);

        public void Handle(NetIncomingMessage message)
        {
            writers[message.MessageType].Log(message.ReadString());
        }
    }
}
