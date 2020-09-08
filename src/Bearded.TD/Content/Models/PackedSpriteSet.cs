using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Textures;
using Bearded.TD.Rendering.Deferred;

namespace Bearded.TD.Content.Models
{
    sealed class PackedSpriteSet
    {
        public ReadOnlyCollection<Texture> Textures { get; }
        public ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> MeshBuilder { get; }
        private readonly IDictionary<string, ISprite> sprites;

        public IEnumerable<(string Id, ISprite Sprite)> All => sprites.Select(kvp => (kvp.Key, kvp.Value));

        public PackedSpriteSet(IEnumerable<Texture> textures, ExpandingIndexedTrianglesMeshBuilder<UVColorVertex> meshBuilder,
            IDictionary<string, ISprite> sprites)
        {
            Textures = textures.ToList().AsReadOnly();
            MeshBuilder = meshBuilder;
            this.sprites = sprites;
        }

        public ISprite GetSprite(string name)
        {
            return sprites[name];
        }
    }
}
