using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using Lidgren.Network;

// ReSharper disable RedundantAssignment

namespace Bearded.TD.Networking.Serialization
{
    sealed class NetBufferReader : INetBufferStream
    {
        private readonly NetBuffer buffer;

        public NetBufferReader(NetBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void Serialize(ref byte[] bytes)
        {
            SerializeArrayCount(ref bytes);
            buffer.ReadBytes(bytes.Length, out bytes);
        }

        public void Serialize(ref int i) => i = buffer.ReadInt32();
        public void Serialize(ref byte b) => b = buffer.ReadByte();

        public void Serialize(ref string s) => s = buffer.ReadString();
        public void Serialize(ref float f) => f = buffer.ReadSingle();

        public void Serialize<T>(ref T t)
            where T : struct
            => buffer.Read(out t);

        public void Serialize<T>(ref Id<T> t) => t = new Id<T>(buffer.ReadInt32());

        public void Serialize<T>(ref ICollection<Id<T>> collection)
        {
            var collectionSize = buffer.ReadInt32();
            collection = new List<Id<T>>();
            for (var i = 0; i < collectionSize; i++)
                collection.Add(new Id<T>(buffer.ReadInt32()));
        }

        public void Serialize(ref Color color) => color = new Color(buffer.ReadUInt32());

        public void Serialize(ref Color? color, uint nullValue = 0)
        {
            var val = buffer.ReadUInt32();
            if (val == nullValue)
                color = null;
            else
                color = new Color(val);
        }

        public void SerializeArrayCount<T>(ref T[] array) => array = new T[buffer.ReadInt32()];

        public void Serialize(ref ModAwareId modAwareId)
        {
            var modId = buffer.ReadString();
            var entityId = buffer.ReadString();
            modAwareId = modId == "" || entityId == "" ? ModAwareId.Invalid : new ModAwareId(modId, entityId);
        }

        public void Serialize(ref Unit unit) => unit = new Unit(buffer.ReadSingle());
    }
}
