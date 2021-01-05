using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Vertices;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Loading;

namespace Bearded.TD.Content.Models
{

    // TODO: this should probably be in Rendering.Loading and exposed via an interface, like ISprite
    sealed class PackedSpriteSet : IDisposable
    {
        private readonly ImmutableArray<TextureUniform> textures;
        private readonly IDictionary<string, ISprite> sprites;

        // TODO: extract these. sprite sets should only be the textures and templates for drawing sprites
        // someone else links a sprite set up with a shader and vertex format to make it drawable
        // see WithVertex() below for a start!
        private readonly Shader shader;
        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> MeshBuilder { get; }

        public PackedSpriteSet(
            IEnumerable<TextureUniform> textures,
            ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> meshBuilder,
            IDictionary<string, ISprite> sprites,
            Shader shader)
        {
            this.textures = textures.ToImmutableArray();
            MeshBuilder = meshBuilder;
            this.sprites = sprites;
            this.shader = shader;
        }

        public ISprite GetSprite(string name)
        {
            return sprites[name];
        }

        public DrawableSpriteSet<TVertex, TVertexData> WithVertex<TVertex, TVertexData>(
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex)
            where TVertex : struct, IVertexData
        {
            return DrawableSpriteSet.Create(textures, sprites, shader, createVertex);
        }

        public IRenderer CreateRendererWithSettings(params IRenderSetting[] additionalSettings)
        {
            var renderer = BatchedRenderer.From(
                MeshBuilder.ToRenderable(),
                textures.Concat(additionalSettings)
                );

            shader.RendererShader.UseOnRenderer(renderer);

            return renderer;
        }

        // TODO: Not obvious what should disposed here and what not - maybe better to just have a dispose bag per mod?
        public void Dispose()
        {
            foreach (var textureUniform in textures)
            {
                textureUniform.Value.Dispose();
            }
            MeshBuilder.Dispose();
        }
    }
}
