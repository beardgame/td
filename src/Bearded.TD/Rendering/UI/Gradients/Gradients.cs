using System;
using Bearded.Graphics;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Textures;
using OpenTK.Graphics.OpenGL;

namespace Bearded.TD.Rendering.UI.Gradients;

sealed class Gradients : IDisposable
{
    private readonly Buffer<GradientStop> buffer;
    private readonly BufferStream<GradientStop> bufferStream;
    private readonly BufferTexture bufferTexture;

    public Gradients()
    {
        buffer = new Buffer<GradientStop>();
        bufferStream = new BufferStream<GradientStop>(buffer);
        bufferTexture = BufferTexture.ForBuffer(buffer, SizedInternalFormat.Rg32ui);
    }

    public IRenderSetting TextureUniform(string name, TextureUnit textureUnit = TextureUnit.Texture0)
        => new BufferTextureUniform(name, textureUnit, bufferTexture);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
    {
        var id = new GradientId((uint)bufferStream.Count);
        bufferStream.Add(stops);
        return id;
    }

    public void Flush() => bufferStream.FlushIfDirty(BufferTarget.TextureBuffer);

    public void Clear() => bufferStream.Clear();

    public void Dispose()
    {
        buffer.Dispose();
        bufferTexture.Dispose();
    }
}
