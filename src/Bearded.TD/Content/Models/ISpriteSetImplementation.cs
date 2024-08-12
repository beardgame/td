using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Content.Models;

interface ISpriteSetImplementation : IDisposable
{
    SpriteParameters GetSpriteParameters(string name);

    DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
        SpriteSet spriteSet, IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        CreateVertex<TVertex, TVertexData> createVertex,
        Shader shader)
        where TVertex : struct, IVertexData;

    (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
        IDrawableRenderers drawableRenderers,
        CreateVertex<TVertex, TVertexData> createVertex,
        Shader shader,
        params IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData;
}
