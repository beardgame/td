using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering;

interface ISpriteRenderers
{
    TDrawableType GetOrCreateDrawableSpriteSetFor<TDrawableType>(
        IDrawableTemplate spriteSet, Shader shader, SpriteDrawGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable;

    public IRenderer CreateCustomRendererFor(
        IDrawable drawable,
        IRenderSetting[] customRenderSettings);

    void RenderDrawGroup(SpriteDrawGroup group);
    void Dispose();
    void ClearAll();
}
