using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Commands;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Synchronization;

static class SerializerConverters
{
    [SerializerConverter]
    public static readonly ISerializerConverter<GameObject, Id<GameObject>, GameInstance> GameObject =
        new SerializerConverter<GameObject, Id<GameObject>, GameInstance>(
                obj => obj.FindId(),
                (id, game) => game.State.Find(id)
            );
}

static class Serializers
{
    [Serializer]
    public static void Serialize(ISerializerBufferStream stream, ref TimeSpan value)
    {
        var v = value.NumericValue;
        stream.Serialize(ref v);
        value = v.S();
    }
}

