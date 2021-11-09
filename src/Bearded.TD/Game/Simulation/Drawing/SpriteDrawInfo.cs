using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Game.Simulation.Drawing
{
    static class SpriteDrawInfo
    {
        public static SpriteDrawInfo<TVertex, TVertexData> From<TVertex, TVertexData>(
            ISpriteBlueprint sprite, DrawableSprite<TVertex, TVertexData>.CreateSprite create, Shader shader)
            where TVertex : struct, IVertexData
        {
            return new SpriteDrawInfo<TVertex, TVertexData>(sprite, create, shader);
        }
    }

    readonly struct SpriteDrawInfo<TVertex, TVertexData>
        where TVertex : struct, IVertexData
    {
        public ISpriteBlueprint Sprite { get; }
        public DrawableSprite<TVertex, TVertexData>.CreateSprite Create { get; }
        public Shader Shader { get; }

        public SpriteDrawInfo(ISpriteBlueprint sprite, DrawableSprite<TVertex, TVertexData>.CreateSprite create, Shader shader)
        {
            Sprite = sprite;
            Create = create;
            Shader = shader;
        }
    }
}
