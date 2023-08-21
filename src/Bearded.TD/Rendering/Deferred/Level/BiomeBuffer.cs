using System.Runtime.InteropServices;
using Bearded.TD.Rendering.Tilemaps;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Deferred.Level;

[StructLayout(LayoutKind.Sequential)]
readonly struct BiomeIds
{
    private readonly byte biomeIdSelf;
    private readonly byte biomeIdRight;
    private readonly byte biomeIdBottomRight;
    private readonly byte biomeIdBottomLeft;

    public BiomeIds(byte biomeIdSelf, byte biomeIdRight, byte biomeIdBottomRight, byte biomeIdBottomLeft)
    {
        this.biomeIdSelf = biomeIdSelf;
        this.biomeIdRight = biomeIdRight;
        this.biomeIdBottomRight = biomeIdBottomRight;
        this.biomeIdBottomLeft = biomeIdBottomLeft;
    }
}

sealed class BiomeBuffer : TilemapBuffer<BiomeIds>
{
    public BiomeBuffer(int radius)
        : base(radius + 1, PixelInternalFormat.Rgba8ui, PixelFormat.RgbaInteger, PixelType.UnsignedByte)
    {

    }
}
