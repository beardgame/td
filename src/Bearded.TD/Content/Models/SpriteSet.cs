using System;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models;

enum DrawOrderGroup
{
    // When adding new groups, make sure the DeferredRenderer knows about them, or they won't render
    SolidLevelDetails,
    LevelDetail,
    Building,
    Unit,
    Particle,
    IgnoreDepth,
    Unknown
}

sealed class SpriteSet : IBlueprint, IDisposable, IDrawableTemplate
{
    private readonly ISpriteSetImplementation sprites;
    public ModAwareId Id { get; }

    public SpriteSet(ModAwareId id, ISpriteSetImplementation sprites)
    {
        Id = id;
        this.sprites = sprites;
    }

    public ISpriteBlueprint GetSprite(string name)
    {
        return new SpriteBlueprint(this, name, sprites.GetSpriteParameters(name));
    }

    public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
        IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex, Shader shader)
        where TVertex : struct, IVertexData
    {
        return sprites.MakeConcreteWith(this, drawableRenderers, drawGroup, drawGroupOrderKey, createVertex, shader);
    }

    public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
        IDrawableRenderers drawableRenderers,
        DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
        Shader shader,
        params IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData
    {
        return sprites.MakeCustomRendererWith(drawableRenderers, createVertex, shader, customRenderSettings);
    }

    public void Dispose()
    {
        sprites.Dispose();
    }
}
