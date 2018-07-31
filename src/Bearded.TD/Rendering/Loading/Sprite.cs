using amulware.Graphics;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Rendering.Loading
{
    class Sprite : ISprite, IHasSurface
    {
        private readonly IndexedSurface<UVColorVertexData> surface;
        private readonly UVRectangle uv;

        public Sprite(IndexedSurface<UVColorVertexData> surface, UVRectangle uv)
        {
            this.surface = surface;
            this.uv = uv;
        }

        Surface IHasSurface.Surface => surface;
    }
}
