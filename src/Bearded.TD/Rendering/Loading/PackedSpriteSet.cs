using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Vertices;

namespace Bearded.TD.Rendering.Loading;

sealed class PackedSpriteSet : ISpriteSetImplementation
{
    private readonly ImmutableArray<TextureUniform> textures;
    private readonly ImmutableDictionary<string, SpriteParameters> sprites;

    public PackedSpriteSet(
        IEnumerable<TextureUniform> textures,
        IDictionary<string, SpriteParameters> sprites)
    {
        this.textures = textures.ToImmutableArray();
        this.sprites = sprites.ToImmutableDictionary();
    }

    public SpriteParameters GetSpriteParameters(string name)
    {
        return sprites[name];
    }

    public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
        SpriteSet spriteSet, IDrawableRenderers drawableRenderers,
        DrawOrderGroup drawGroup, int drawGroupOrderKey,
        CreateVertex<TVertex, TVertexData> createVertex,
        Shader shader)
        where TVertex : struct, IVertexData
    {
        return drawableRenderers.GetOrCreateDrawableFor(
            spriteSet, shader, drawGroup, drawGroupOrderKey,
            () => DrawableSpriteSet.Create(textures, sprites, createVertex)
        );
    }

    public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
        IDrawableRenderers drawableRenderers,
        CreateVertex<TVertex, TVertexData> createVertex,
        Shader shader,
        params IRenderSetting[] customRenderSettings)
        where TVertex : struct, IVertexData
    {
        // TODO: who is responsible for cleaning these up?
        var drawable = DrawableSpriteSet.Create(textures, sprites, createVertex);
        var renderer = drawableRenderers.CreateUnregisteredRendererFor(drawable, customRenderSettings);
        shader.RendererShader.UseOnRenderer(renderer);

        return (drawable, renderer);
    }

    // TODO: Not obvious what should disposed here and what not - maybe better to just have a dispose bag per mod?
    public void Dispose()
    {
        foreach (var textureUniform in textures)
        {
            textureUniform.Value.Dispose();
        }
    }
}
