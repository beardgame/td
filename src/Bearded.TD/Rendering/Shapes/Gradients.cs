using System;
using Bearded.Graphics;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using Bearded.TD.UI.Shapes;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.Shapes;

sealed class Gradients : IDisposable
{
    private readonly Buffer<IndexedGradientStop> buffer;
    private readonly BufferStream<IndexedGradientStop> bufferStream;
    private readonly BufferTexture bufferTexture;

    public Gradients()
    {
        buffer = new Buffer<IndexedGradientStop>();
        bufferStream = new BufferStream<IndexedGradientStop>(buffer);
        bufferTexture = BufferTexture.ForBuffer(buffer, SizedInternalFormat.Rgb32ui);
    }

    public IRenderSetting TextureUniform(string name, TextureUnit textureUnit = TextureUnit.Texture0)
        => new BufferTextureUniform(name, textureUnit, bufferTexture);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
    {
        var gradient = IndexedGradient.From(stops, stackalloc IndexedGradientStop[stops.Length]);
        return AddGradient(gradient);
    }

    public GradientId AddGradient(in IndexedGradient gradient)
    {
        if(gradient.Count == 0)
            return new GradientId(0);

        var id = new GradientId((uint)bufferStream.Count);
        bufferStream.Add(gradient.Stops);
        return id;
    }

    public void Flush()
    {
        if (bufferStream.Count == 0 || !bufferStream.IsDirty)
            return;
        bufferStream.FlushIfDirty(BufferTarget.TextureBuffer);
        using var target = bufferTexture.Bind();
        target.AttachBuffer(buffer, SizedInternalFormat.Rgb32ui);
    }

    public void Clear()
    {
        bufferStream.Clear();
        bufferStream.Add(default(IndexedGradientStop));
    }

    public void Dispose()
    {
        buffer.Dispose();
        bufferTexture.Dispose();
    }
}
