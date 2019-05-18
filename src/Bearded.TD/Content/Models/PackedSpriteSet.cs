using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Content.Models
{
    sealed class PackedSpriteSet
    {
        public Texture Texture { get; }
        public IndexedSurface<UVColorVertexData> Surface { get; }
        private readonly IDictionary<string, ISprite> sprites;

        public IEnumerable<(string Id, ISprite Sprite)> All => sprites.Select(kvp => (kvp.Key, kvp.Value));

        public PackedSpriteSet(Texture texture, IndexedSurface<UVColorVertexData> surface,
            IDictionary<string, ISprite> sprites)
        {
            Texture = texture;
            Surface = surface;
            this.sprites = sprites;
        }

        public ISprite GetSprite(string name)
        {
            return sprites[name];
        }
    }
}
