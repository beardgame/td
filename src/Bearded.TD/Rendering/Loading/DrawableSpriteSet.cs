using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Loading;

static class DrawableSpriteSet
{
    public static DrawableSpriteSet<TVertex, TVertexData> Create<TVertex, TVertexData>(
        ImmutableArray<TextureUniform> textures,
        ImmutableDictionary<string, SpriteParameters> baseSprites,
        CreateVertex<TVertex, TVertexData> createVertex)
        where TVertex : struct, IVertexData
    {
        var meshBuilder = new ExpandingIndexedTrianglesMeshBuilder<TVertex>();
        var sprites = baseSprites.ToDictionary(
            kvp => kvp.Key,
            kvp => new DrawableSprite<TVertex, TVertexData>(meshBuilder, createVertex, kvp.Value)
        );
        return new DrawableSpriteSet<TVertex, TVertexData>(meshBuilder, sprites, textures);
    }
}

sealed class DrawableSpriteSet<TVertex, TVertexData> : IDrawable
    where TVertex : struct, IVertexData
{
    private readonly ExpandingIndexedTrianglesMeshBuilder<TVertex> meshBuilder;
    private readonly Dictionary<string, DrawableSprite<TVertex, TVertexData>> sprites;
    private readonly ImmutableArray<TextureUniform> textures;

    // TODO: might be worth hiding TVertex behind an IMeshBuilder interface
    // and/or hide this class behind a IDrawableSpriteSet<TVertexData> - except.. do we actually use it anywhere?
    public DrawableSpriteSet(
        ExpandingIndexedTrianglesMeshBuilder<TVertex> meshBuilder,
        Dictionary<string, DrawableSprite<TVertex, TVertexData>> sprites,
        ImmutableArray<TextureUniform> textures)
    {
        this.meshBuilder = meshBuilder;
        this.sprites = sprites;
        this.textures = textures;
    }

    public IDrawableSprite<TVertex, TVertexData> GetSprite(string name)
    {
        return sprites[name];
    }

    public IEnumerable<(string Name, DrawableSprite<TVertex, TVertexData> Sprite)> AllSprites
        => sprites.Select(kvp => (kvp.Key, kvp.Value));

    public IRenderer CreateRendererWithSettings(params IRenderSetting[] additionalSettings)
    {
        return CreateRendererWithSettings(additionalSettings.AsEnumerable());
    }

    public IRenderer CreateRendererWithSettings(IEnumerable<IRenderSetting> additionalSettings)
    {
        var renderer = BatchedRenderer.From(
            meshBuilder.ToRenderable(),
            textures.Concat(additionalSettings)
        );

        return renderer;
    }

    // TODO: should this be here? if not, where?
    public void Clear()
    {
        meshBuilder.Clear();
    }

    public void Dispose()
    {
        meshBuilder.Dispose();
    }
}
