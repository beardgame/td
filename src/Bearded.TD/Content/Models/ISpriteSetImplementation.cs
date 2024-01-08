using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models;

internal interface ISpriteSetImplementation : IDisposable
{
    SpriteParameters GetSpriteParameters(string name);

    DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
        SpriteSet spriteSet, IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
        Shader shader)
        where TVertex : struct, IVertexData;

    (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
        IDrawableRenderers drawableRenderers,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
        Shader shader,
        params IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData;
}
