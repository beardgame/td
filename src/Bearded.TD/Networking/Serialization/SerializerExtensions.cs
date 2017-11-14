using Bearded.TD.Commands;

namespace Bearded.TD.Networking.Serialization
{
    static class SerializerExtensions
    {
        public static IRequest<TContext, TSender> Read<TContext, TSender>(
            this IRequestSerializer<TContext, TSender> me, NetBufferReader stream, TContext context, TSender sender)
        {
            me.Serialize(stream);
            return me.GetRequest(context, sender);
        }

        public static ICommand<TContext> Read<TContext>(
            this ICommandSerializer<TContext> me, NetBufferReader stream, TContext context)
        {
            me.Serialize(stream);
            return me.GetCommand(context);
        }

        public static void Write<TContext, TSender>(this IRequest<TContext, TSender> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }

        public static void Write<TContext>(this ICommand<TContext> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }
    }
}