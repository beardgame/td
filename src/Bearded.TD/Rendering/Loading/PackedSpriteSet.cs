using System.Collections.Generic;
using System.Collections.Immutable;
using amulware.Graphics.Rendering;
using amulware.Graphics.RenderSettings;
using amulware.Graphics.Vertices;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering.Loading
{
    sealed class PackedSpriteSet : ISpriteSetImplementation
    {
        private readonly ImmutableArray<TextureUniform> textures;
        private readonly ImmutableDictionary<string, SpriteParameters> sprites;

        // TODO: this should only serve as a 'default shader' and maybe not even that,
        // but not having it we need to specify it everywhere where we refer to the sprite and it gets a mess
        // the implicit association with between shader code and vertex fields is also not represented anywhere :/
        private readonly Shader shader;

        public PackedSpriteSet(
            IEnumerable<TextureUniform> textures,
            IDictionary<string, SpriteParameters> sprites,
            Shader shader)
        {
            this.textures = textures.ToImmutableArray();
            this.sprites = sprites.ToImmutableDictionary();
            this.shader = shader;
        }

        public SpriteParameters GetSpriteParameters(string name)
        {
            return sprites[name];
        }

        public DrawableSpriteSet<TVertex, TVertexData> MakeConcreteWith<TVertex, TVertexData>(
            SpriteSet spriteSet, SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex)
            where TVertex : struct, IVertexData
        {
            return spriteRenderers.GetOrCreateDrawableSpriteSetFor(
                spriteSet,
                () => DrawableSpriteSet.Create(textures, sprites, shader, createVertex)
            );
        }

        public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
            SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
            params IRenderSetting[] customRenderSettings)
            where TVertex : struct, IVertexData
        {
            // TODO: who is responsible for cleaning these up?
            var drawable = DrawableSpriteSet.Create(textures, sprites, shader, createVertex);
            var renderer = spriteRenderers.CreateCustomRendererFor(drawable, customRenderSettings);
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
}
