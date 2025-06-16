using Bearded.TD.Content.Mods;
using Bearded.TD.Shared.Commands;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Networking.Serialization;

interface INetBufferStream : ISerializerBufferStream
{
    // Consider removing later, they are just here for compatibility across interfaces.
    new void Serialize(ref byte[] bytes) => ((ISerializerBufferStream)this).Serialize(ref bytes);
    new void Serialize(ref string s) => ((ISerializerBufferStream)this).Serialize(ref s);
    new void Serialize(ref int i) => ((ISerializerBufferStream)this).Serialize(ref i);
    new void Serialize(ref byte b) => ((ISerializerBufferStream)this).Serialize(ref b);
    new void Serialize(ref float f) => ((ISerializerBufferStream)this).Serialize(ref f);
    new void Serialize(ref double f) => ((ISerializerBufferStream)this).Serialize(ref f);

    void Serialize<T>(ref T t)
        where T : struct;
    void Serialize<T>(ref Id<T> t);

    void SerializeArrayCount<T>(ref T[] array);

    void Serialize(ref ModAwareId modAwareId);
    void Serialize(ref Unit unit);
}
