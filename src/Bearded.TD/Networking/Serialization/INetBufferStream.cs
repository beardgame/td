using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Networking.Serialization;

interface INetBufferStream
{
    void Serialize(ref byte[] bytes);

    void Serialize(ref int i);
    void Serialize(ref byte b);
    void Serialize(ref string s);
    void Serialize(ref float f);
    void Serialize<T>(ref T t)
        where T : struct;
    void Serialize<T>(ref Id<T> t);

    void Serialize<T>(ref ICollection<Id<T>> collection);

    void Serialize(ref Color color);
    void Serialize(ref Color? color, uint nullValue = 0);

    void SerializeArrayCount<T>(ref T[] array);

    void Serialize(ref ModAwareId modAwareId);
    void Serialize(ref Unit unit);
}