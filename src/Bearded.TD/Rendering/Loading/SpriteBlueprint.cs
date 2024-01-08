using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering.Loading;

sealed class SpriteBlueprint : ISpriteBlueprint
{
    // TODO: we don't really need the name, might be better to either use an index or the real sprite
    // though not sure how using the real sprite works with the drawable sprite set making a copy of all sprites
    private readonly string name;
    private readonly SpriteSet spriteSet;

    public SpriteParameters SpriteParameters { get; }

    public SpriteBlueprint(SpriteSet spriteSet, string name, SpriteParameters parameters)
    {
        this.name = name;
        SpriteParameters = parameters;
        this.spriteSet = spriteSet;
    }

    public IDrawableSprite<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex, Shader shader)
        where TVertex : struct, IVertexData
    {
        return spriteSet.MakeConcreteWith(drawableRenderers, drawGroup, drawGroupOrderKey, createVertex, shader).GetSprite(name);
    }
}
