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

        // TODO: this should only serve as a 'default shader' and maybe not even that,
        // but not having it we need to specify it everywhere where we refer to the sprite and it gets a mess
        // the implicit association with between shader code and vertex fields is also not represented anywhere :/
        private readonly Shader shader;

        public PackedSpriteSet(
            IEnumerable<TextureUniform> textures,
            IDictionary<string, ISprite> sprites,
            Shader shader)
        {
            this.textures = textures.ToImmutableArray();
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
