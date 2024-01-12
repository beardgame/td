using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Content.Models;

interface ISpriteBlueprint
{
    SpriteParameters SpriteParameters { get; }

    IDrawableSprite<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        CreateVertex<TVertex, TVertexData> createVertex, Shader shader)
        where TVertex : struct, IVertexData;
}
