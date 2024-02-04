using System;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.Tiles;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Tilemaps;

abstract class TilemapBuffer<T> : IDisposable
    where T : struct
{
    private readonly int radius;
    private readonly PixelInternalFormat internalPixelFormat;
    private readonly PixelFormat pixelFormat;
    private readonly PixelType pixelType;
    private readonly int width;
    private readonly T[] pixels;
    private readonly Texture texture;
    private bool dataUploaded;

    public TilemapBuffer(
        int radius,
        PixelInternalFormat internalPixelFormat,
        PixelFormat pixelFormat,
        PixelType pixelType)
    {
        this.radius = radius;
        this.internalPixelFormat = internalPixelFormat;
        this.pixelFormat = pixelFormat;
        this.pixelType = pixelType;
        width = radius * 2 + 1;

        pixels = new T[width * width];

        texture = new Texture();
        using var target = texture.Bind();
        target.SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        target.SetWrapMode(TextureWrapMode.ClampToBorder, TextureWrapMode.ClampToBorder);
    }

    public IntUniform GetRadiusUniform(string name) => new(name, radius);
    public TextureUniform GetTextureUniform(string name, TextureUnit unit) => new(name, unit, texture);

    public void SetTile(Tile tile, T value)
    {
        SetTile(tile.X, tile.Y, value);
    }

    public void SetTile(int x, int y, T value)
    {
        pixels[x + radius + (y + radius) * width] = value;

        dataUploaded = false;
    }

    public void UploadIfNeeded()
    {
        if (dataUploaded)
            return;

        using var target = texture.Bind();
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalPixelFormat,
            width, width, 0, pixelFormat, pixelType, pixels);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

        dataUploaded = true;
    }

    public void Dispose()
    {
        texture.Dispose();
    }
}
