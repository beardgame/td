using amulware.Graphics;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Rendering.Loading
{
    class Sprite : ISprite
    {
        private IndexedSurface<UVColorVertexData> surface;
        private UVRectangle uv;

        public Sprite(IndexedSurface<UVColorVertexData> surface, UVRectangle uv)
        {
            this.surface = surface;
            this.uv = uv;
        }
    }

}
