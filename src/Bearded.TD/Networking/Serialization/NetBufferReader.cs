using Bearded.TD.Utilities;
using Lidgren.Network;

namespace Bearded.TD.Networking.Serialization
{
    class NetBufferReader : INetBufferStream
    {
        private readonly NetBuffer buffer;

        public NetBufferReader(NetBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void Serialize(ref byte b) => b = buffer.ReadByte();

        public void Serialize(ref int i) => i = buffer.ReadInt32();

        public void Serialize(ref string s) => s = buffer.ReadString();

        public void Serialize<T>(ref T t)
            where T : struct
            => buffer.Read<T>(out t);

        public void Serialize<T>(ref Id<T> t) => t = new Id<T>(buffer.ReadInt32());
    }
}