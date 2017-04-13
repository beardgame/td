using Bearded.TD.Utilities;

namespace Bearded.TD.Networking.Serialization
{
    interface INetBufferStream
    {
        void Serialize(ref int i);
        void Serialize(ref string s);
        void Serialize<T>(ref T t)
            where T : struct;
        void Serialize<T>(ref Id<T> t);
    }
}