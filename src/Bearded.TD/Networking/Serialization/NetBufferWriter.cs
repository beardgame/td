using System.Collections.Generic;
using amulware.Graphics;
using Bearded.TD.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Serialization
{
    class NetBufferWriter : INetBufferStream
    {
        private readonly NetBuffer buffer;

        public NetBufferWriter(NetBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void Serialize(ref int i) => buffer.Write(i);
        public void Serialize(ref byte b) => buffer.Write(b);

        public void Serialize(ref string s) => buffer.Write(s);

        public void Serialize<T>(ref T t)
            where T : struct
            => buffer.Write(t);

        public void Serialize<T>(ref Id<T> t) => buffer.Write(t.Value);

        public void Serialize<T>(ref ICollection<Id<T>> collection)
        {
            buffer.Write(collection.Count);
            foreach (var i in collection)
                buffer.Write(i.Value);
        }

        public void Serialize(ref Color color) => buffer.Write(color.ARGB);

        public void Serialize(ref Color? color, uint nullValue = 0) => buffer.Write(color?.ARGB ?? nullValue);

        public void SerializeArrayCount<T>(ref T[] array)
        {
            buffer.Write(array.Length);
        }
    }
}