using System.Collections.Generic;
using Bearded.TD.Utilities;

namespace Bearded.TD.Networking.Serialization
{
    interface INetBufferStream
    {
        void Serialize(ref int i);
        void Serialize(ref byte b);
        void Serialize(ref string s);
        void Serialize<T>(ref T t)
            where T : struct;
        void Serialize<T>(ref Id<T> t);

        void Serialize<T>(ref ICollection<Id<T>> collection);
    }
}