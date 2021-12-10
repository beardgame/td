using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Game.Simulation.Drawing;

static class SpriteDrawInfo
{
    public static SpriteDrawInfo<UVColorVertex, Color> ForUVColor(
        GameState game,
        ISpriteBlueprint sprite,
        Shader? shader = null,
        SpriteDrawGroup drawGroup = SpriteDrawGroup.Particle,
        int drawGroupOrderKey = 0)
    {
        return FromWithFallbackShader(game, shader, sprite, UVColorVertex.Create, drawGroup, drawGroupOrderKey);
    }

    public static SpriteDrawInfo<TVertex, TVertexData> FromWithFallbackShader<TVertex, TVertexData>(
        GameState game, Shader? shader, ISpriteBlueprint sprite,
        DrawableSprite<TVertex, TVertexData>.CreateSprite create,
        SpriteDrawGroup drawGroup, int drawGroupOrderKey = 0)
        where TVertex : struct, IVertexData
    {
        shader ??= game.Meta.Blueprints.Shaders[ModAwareId.ForDefaultMod("default-sprite")];

        return From(sprite, create, shader, drawGroup, drawGroupOrderKey);
    }

    public static SpriteDrawInfo<TVertex, TVertexData> From<TVertex, TVertexData>(
        ISpriteBlueprint sprite, DrawableSprite<TVertex, TVertexData>.CreateSprite create, Shader shader,
        SpriteDrawGroup drawGroup, int drawGroupOrderKey = 0)
        where TVertex : struct, IVertexData
    {
        return new SpriteDrawInfo<TVertex, TVertexData>(sprite, create, shader, drawGroup, drawGroupOrderKey);
    }
}

readonly struct SpriteDrawInfo<TVertex, TVertexData>
    where TVertex : struct, IVertexData
{
    public ISpriteBlueprint Sprite { get; }
    public DrawableSprite<TVertex, TVertexData>.CreateSprite Create { get; }
    public Shader Shader { get; }
    public SpriteDrawGroup DrawGroup { get; }
    public int DrawGroupOrderKey { get; }

    public SpriteDrawInfo(
        ISpriteBlueprint sprite,
        DrawableSprite<TVertex, TVertexData>.CreateSprite create,
        Shader shader,
        SpriteDrawGroup drawGroup, int drawGroupOrderKey)
    {
        Sprite = sprite;
        Create = create;
        Shader = shader;
        DrawGroup = drawGroup;
        DrawGroupOrderKey = drawGroupOrderKey;
    }
}