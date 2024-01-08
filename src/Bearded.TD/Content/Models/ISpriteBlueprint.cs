using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models;

interface ISpriteBlueprint
{
    SpriteParameters SpriteParameters { get; }

    IDrawableSprite<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex, Shader shader)
        where TVertex : struct, IVertexData;
}
