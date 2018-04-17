using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    static class SerializerExtensions
    {
        public static IRequest<TObject> Read<TObject>(this IRequestSerializer<TObject> me, NetBufferReader stream, TObject context)
        {
            me.Serialize(stream);
            return me.GetRequest(context);
        }

        public static ISerializableCommand<TObject> Read<TObject>(this ICommandSerializer<TObject> me, NetBufferReader stream, TObject context)
        {
            me.Serialize(stream);
            return me.GetCommand(context);
        }

        public static void Write<TObject>(this IRequest<TObject> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }

        public static void Write<TObject>(this ISerializableCommand<TObject> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }
    }
}