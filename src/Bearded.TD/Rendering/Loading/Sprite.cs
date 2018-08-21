using amulware.Graphics;
using Bearded.TD.Mods.Models;
using OpenTK;

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

        public void Draw(Vector3 position, Color color, float size)
        {
            var left = position.X - size;
            var right = position.X + size;

            var top = position.Y - size;
            var bottom = position.Y + size;

            var z = position.Z;

            surface.AddQuad(
                v(left, top, z, uv.TopLeft, color),
                v(right, top, z, uv.TopRight, color),
                v(right, bottom, z, uv.BottomRight, color),
                v(left, bottom, z, uv.BottomLeft, color)
            );
        }

        private static UVColorVertexData v(float x, float y, float z, Vector2 uv, Color color)
            => new UVColorVertexData(x, y, z, uv, color);
    }
}
