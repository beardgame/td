using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Mods.Models
{
    sealed class PackedSpriteSet
    {
        public Texture Texture { get; }
        private readonly IDictionary<string, ISprite> sprites;

        public IEnumerable<(string Id, ISprite Sprite)> All => sprites.Select(kvp => (kvp.Key, kvp.Value));

        public PackedSpriteSet(Texture texture, IDictionary<string, ISprite> sprites)
        {
            Texture = texture;
            this.sprites = sprites;
        }

        public ISprite GetSprite(string name)
        {
            return sprites[name];
        }
    }
}
