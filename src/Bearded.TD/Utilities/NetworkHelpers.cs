using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lidgren.Network;

namespace Bearded.TD.Utilities;

static class NetworkHelpers
{
    public static void WriteStruct<T>(this NetBuffer buffer, in T s)
        where T : struct
    {
        var size = Marshal.SizeOf(typeof(T));
        var array = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(s, ptr, true);
        Marshal.Copy(ptr, array, 0, size);
        Marshal.FreeHGlobal(ptr);
        buffer.Write(array);
    }

    public static void ReadStruct<T>(this NetBuffer buffer, out T s)
        where T : struct
    {
        var size = Marshal.SizeOf(typeof(T));
        var array = buffer.ReadBytes(size);
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(array, 0, ptr, size);
        // ReSharper disable once PossibleNullReferenceException
        s = (T)Marshal.PtrToStructure(ptr, typeof(T));
        Marshal.FreeHGlobal(ptr);
    }

    public static void WriteEnum<T>(this NetBuffer buffer, ref T e)
        where T : struct
    {
        var i = Unsafe.As<T, int>(ref e);
        buffer.Write(i);
    }

    public static void ReadEnum<T>(this NetBuffer buffer, out T e)
        where T : struct
    {
        var i = buffer.ReadInt32();
        e = Unsafe.As<int, T>(ref i);
    }
}
