using System;
using Bearded.Graphics;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering;

abstract class TextureBuffer<T> : IDisposable
    where T : struct
{
    private readonly SizedInternalFormat pixelFormat;
    private readonly bool reserveDefault;
    private readonly Buffer<T> buffer;
    private readonly BufferStream<T> stream;
    private readonly BufferTexture bufferTexture;

    private bool isEmpty;

    protected TextureBuffer(SizedInternalFormat pixelFormat, bool reserveDefault = true)
    {
        this.pixelFormat = pixelFormat;
        this.reserveDefault = reserveDefault;
        buffer = new Buffer<T>();
        stream = new BufferStream<T>(buffer);
        Clear();
        bufferTexture = BufferTexture.ForBuffer(buffer, pixelFormat);
    }

    public IRenderSetting TextureUniform(string name, TextureUnit textureUnit = TextureUnit.Texture0)
        => new BufferTextureUniform(name, textureUnit, bufferTexture);

    protected int Add(ReadOnlySpan<T> items)
    {
        var id = stream.Count;
        stream.Add(items);
        isEmpty = false;
        return id;
    }

    protected int Add(in T item)
    {
        var id = stream.Count;
        stream.Add(item);
        isEmpty = false;
        return id;
    }

    public void Flush()
    {
        if (!stream.IsDirty)
            return;
        stream.FlushIfDirty(BufferTarget.TextureBuffer);
        using var target = bufferTexture.Bind();
        target.AttachBuffer(buffer, pixelFormat);
    }

    public void Clear()
    {
        if (isEmpty)
            return;

        stream.Clear();
        if (reserveDefault)
            stream.Add(default(T));

        isEmpty = true;
    }

    public void Dispose()
    {
        buffer.Dispose();
        bufferTexture.Dispose();
    }
}
