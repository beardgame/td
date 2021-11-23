using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Game.Simulation.Drawing
{
    static class SpriteDrawInfo
    {
        public static SpriteDrawInfo<UVColorVertex, Color> ForUVColor(
            GameState game, Shader? shader, ISpriteBlueprint sprite)
        {
            return FromWithFallbackShader(game, shader, sprite, UVColorVertex.Create);
        }

        public static SpriteDrawInfo<TVertex, TVertexData> FromWithFallbackShader<TVertex, TVertexData>(
            GameState game, Shader? shader, ISpriteBlueprint sprite,
            DrawableSprite<TVertex, TVertexData>.CreateSprite create)
            where TVertex : struct, IVertexData
        {
            shader ??= game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("default-sprite")];

            return From(sprite, create, shader);
        }

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
