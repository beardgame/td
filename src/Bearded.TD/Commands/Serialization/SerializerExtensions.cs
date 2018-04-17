using Bearded.TD.Game;
using Bearded.TD.Game.Players;
using Bearded.TD.Networking.Serialization;

namespace Bearded.TD.Commands.Serialization
{
    static class SerializerExtensions
    {
        public static IRequest<GameInstance> Read(this IRequestSerializer<GameInstance> me, NetBufferReader stream, GameInstance game, Player sender)
        {
            me.Serialize(stream);
            return me.GetRequest(game, sender);
        }

        public static ISerializableCommand<GameInstance> Read(this ICommandSerializer<GameInstance> me, NetBufferReader stream, GameInstance game)
        {
            me.Serialize(stream);
            return me.GetCommand(game);
        }

        public static void Write(this IRequest<GameInstance> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }

        public static void Write(this ISerializableCommand<GameInstance> me, NetBufferWriter stream)
        {
            me.Serializer.Serialize(stream);
        }
    }
}