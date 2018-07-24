using System.Collections.Generic;
using amulware.Graphics;

namespace Bearded.TD.Mods.Models
{
    sealed class PackedSpriteSet
    {
        public Texture Texture { get; }
        private readonly IDictionary<string, UVRectangle> spriteCoordinates;

        public PackedSpriteSet(Texture texture, IDictionary<string, UVRectangle> spriteCoordinates)
        {
            Texture = texture;
            this.spriteCoordinates = spriteCoordinates;
        }

        public UVRectangle GetSpriteCoordinates(string name)
        {
            return spriteCoordinates[name];
        }
    }
}
