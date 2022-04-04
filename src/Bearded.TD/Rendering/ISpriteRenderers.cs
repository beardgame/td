using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Rendering;

interface ISpriteRenderers
{
    DrawableSpriteSet<TVertex, TVertexData> GetOrCreateDrawableSpriteSetFor<TVertex, TVertexData>(
        SpriteSet spriteSet,
        Shader shader,
        SpriteDrawGroup drawGroup,
        int drawGroupOrderKey,
        Func<DrawableSpriteSet<TVertex, TVertexData>> createDrawable)
        where TVertex : struct, IVertexData;

    IRenderer CreateCustomRendererFor<TVertex, TVertexData>(
        DrawableSpriteSet<TVertex, TVertexData> drawable,
        IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData;

    void RenderDrawGroup(SpriteDrawGroup group);
    void Dispose();
    void ClearAll();
}
