using Bearded.TD.Commands;
using Bearded.TD.Game;

namespace Bearded.TD.Networking.Serialization
{
    static class SerializerExtensions
    {
        public static IRequest Read(this IRequestSerializer me, NetBufferReader stream, GameInstance game)
        {
            me.Serialize(stream);
            return me.GetRequest(game);
        }

        public static ICommand Read(this ICommandSerializer me, NetBufferReader stream, GameInstance game)
        {
            me.Serialize(stream);
            return me.GetCommand(game);
        }

        public static void Write(this IRequest me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }

        public static void Write(this ICommand me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }
    }
}