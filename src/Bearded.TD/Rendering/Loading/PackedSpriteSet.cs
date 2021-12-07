using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.Graphics.Rendering;
using Bearded.Graphics.RenderSettings;
using Bearded.Graphics.Vertices;
using Bearded.TD.Content.Models;

namespace Bearded.TD.Rendering.Loading
{
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
            SpriteSet spriteSet, SpriteRenderers spriteRenderers,
            SpriteDrawGroup drawGroup, int drawGroupOrderKey,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
            Shader shader)
            where TVertex : struct, IVertexData
        {
            return spriteRenderers.GetOrCreateDrawableSpriteSetFor(
                spriteSet, shader, drawGroup, drawGroupOrderKey,
                () => DrawableSpriteSet.Create(textures, sprites, shader, createVertex)
            );
        }

        public (DrawableSpriteSet<TVertex, TVertexData>, IRenderer) MakeCustomRendererWith<TVertex, TVertexData>(
            SpriteRenderers spriteRenderers,
            DrawableSprite<TVertex, TVertexData>.CreateSprite createVertex,
            Shader shader,
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
