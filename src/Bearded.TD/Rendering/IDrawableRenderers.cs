using System;
using System.Collections.Generic;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.ShaderManagement;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering;

interface IDrawableRenderers
{
    TDrawableType GetOrCreateDrawableFor<TDrawableType>(
        IDrawableTemplate template, Shader shader, DrawOrderGroup drawGroup, int drawGroupOrderKey,
        Func<TDrawableType> createDrawable)
        where TDrawableType : IDrawable;

    IRenderer CreateAndRegisterRendererFor(IRendererShader shader, DrawOrderGroup drawGroup, int drawGroupOrderKey,
        Func<IEnumerable<IRenderSetting>, IRenderer> createRenderer);

    IRenderer CreateUnregisteredRendererFor(
        IDrawable drawable,
        IRenderSetting[] customRenderSettings);

    IRenderer CreateAndRegisterRendererFor(
        IDrawable drawable,
        DrawOrderGroup group, int drawGroupOrderKey);

    void RegisterRenderer(IRenderer renderer, DrawOrderGroup group, int drawGroupOrderKey);

    void RenderDrawGroup(DrawOrderGroup group);
    void DisposeAll();
    void ClearAll();
}
