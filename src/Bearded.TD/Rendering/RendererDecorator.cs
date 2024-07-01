using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Shading;

namespace Bearded.TD.Rendering;

abstract class RendererDecorator(IRenderer renderer, ImmutableArray<IDisposable>? disposables = null) : IRenderer
{
    protected RendererDecorator(
        IDrawable drawable,
        IEnumerable<IRenderSetting> settings,
        ReadOnlySpan<IDisposable> otherDisposables = default)
        : this(
            drawable.CreateRendererWithSettings(settings),
            [drawable, ..otherDisposables]
            )
    {
    }

    private readonly ImmutableArray<IDisposable> disposables = disposables ?? ImmutableArray<IDisposable>.Empty;

    public virtual void Dispose()
    {
        renderer.Dispose();

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public virtual void SetShaderProgram(ShaderProgram program)
        => renderer.SetShaderProgram(program);

    public virtual void Render()
        => renderer.Render();
}
