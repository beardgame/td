using System;
using Bearded.Graphics;
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
        bufferTexture = BufferTexture.ForBuffer(buffer, SizedInternalFormat.Rg32i);
    }

    public GradientId AddGradient(Gradient gradient) => AddGradient(gradient.Stops);

    public GradientId AddGradient(ReadOnlySpan<GradientStop> stops)
    {
        var id = new GradientId((uint)bufferStream.Count);
        bufferStream.Add(stops);
        return id;
    }

    // TODO: call in pipeline? or introduce a render setting into all consumers that calls this?
    public void Flush() => bufferStream.FlushIfDirty(BufferTarget.TextureBuffer);

    // TODO: call wherever else other things are cleared (in pipeline, if also flushing there?)
    public void Clear() => bufferStream.Clear();

    public void Dispose()
    {
        buffer.Dispose();
        bufferTexture.Dispose();
    }
}
