using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering;

interface IDrawableRenderers
{
    TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, DrawOrderGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable;

    public IRenderer CreateCustomRendererFor(
        IDrawable drawable,
        IRenderSetting[] customRenderSettings);

    void RenderDrawGroup(DrawOrderGroup group);
    void Dispose();
    void ClearAll();
}
