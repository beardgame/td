using System;
using System.Runtime.InteropServices;
using Lidgren.Network;

namespace Bearded.TD.Utilities
{
    static class NetworkHelpers
    {
        public static void Write<T>(this NetBuffer buffer, T s)
            where T : struct
            => buffer.Write(ref s);

        public static void Write<T>(this NetBuffer buffer, ref T s)
            where T : struct
            => buffer.Write(toByteArray(ref s));

        public static T Read<T>(this NetBuffer buffer)
            where T : struct
        {
            buffer.Read(out T s);
            return s;
        }

        public static void Read<T>(this NetBuffer buffer, out T s)
            where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = buffer.ReadBytes(size);
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            s = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
        }

        public static byte[] ToByteArray<T>(this T s)
            where T : struct
            => toByteArray(ref s);

        private static byte[] toByteArray<T>(ref T s)
            where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var array = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(s, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
            Marshal.FreeHGlobal(ptr);
            return array;
        }

        public static T ToStruct<T>(this byte[] array)
            where T : struct
        {
            array.ToStruct(out T s);
            return s;
        }

        public static void ToStruct<T>(this byte[] array, out T s)
            where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            if(size != array.Length)
                throw new Exception("Size of array and structure do not match.");
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            s = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
        }
    }
}