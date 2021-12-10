using System.Runtime.InteropServices;
using Lidgren.Network;

namespace Bearded.TD.Utilities;

static class NetworkHelpers
{
    public static void Write<T>(this NetBuffer buffer, T s)
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

    public static void Read<T>(this NetBuffer buffer, out T s)
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
}