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

        public void Serialize(ref byte b) => buffer.Write(b);

        public void Serialize(ref int i) => buffer.Write(i);
        public void Serialize(ref byte b) => buffer.Write(b);

        public void Serialize(ref string s) => buffer.Write(s);

        public void Serialize<T>(ref T t)
            where T : struct
            => buffer.Write(t);

        public void Serialize<T>(ref Id<T> t) => buffer.Write(t.Value);
    }
}