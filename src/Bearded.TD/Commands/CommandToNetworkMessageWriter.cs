using Bearded.TD.Commands.Serialization;
using Bearded.TD.Networking.Serialization;
using Lidgren.Network;

namespace Bearded.TD.Commands
{
    static class CommandToNetworkMessageWriter
    {
        public static void WriteCommandToMessage<TObject>(
            ISerializableCommand<TObject> command, NetOutgoingMessage message)
        {
            var serializer = command.Serializer;
            var serializers = Serializers<TObject>.Instance;
            var id = serializers.CommandId(command.Serializer);

            message.Write(id);
            serializer.Serialize(new NetBufferWriter(message));
        }
    }
}
