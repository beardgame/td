using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Content.Models
{
    sealed class PackedSpriteSet
    {
        public ReadOnlyCollection<Texture> Textures { get; }
        public IndexedSurface<UVColorVertexData> Surface { get; }
        private readonly IDictionary<string, ISprite> sprites;

        public IEnumerable<(string Id, ISprite Sprite)> All => sprites.Select(kvp => (kvp.Key, kvp.Value));

        public PackedSpriteSet(IEnumerable<Texture> textures, IndexedSurface<UVColorVertexData> surface,
            IDictionary<string, ISprite> sprites)
        {
            Textures = textures.ToList().AsReadOnly();
            Surface = surface;
            this.sprites = sprites;
        }

        public ISprite GetSprite(string name)
        {
            return sprites[name];
        }
    }
}
