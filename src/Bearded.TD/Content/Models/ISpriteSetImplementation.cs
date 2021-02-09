using System;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Vertices;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models
{
    internal interface ISpriteSetImplementation : IDisposable
    {
        SpriteParameters GetSpriteParameters(string name);

        DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
            SpriteSet spriteSet, SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex)
            where TVertex : struct, IVertexData;

        (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
            SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
            params IRenderSetting[] customRenderSettings)
            where TVertex : struct, IVertexData;
    }
}
