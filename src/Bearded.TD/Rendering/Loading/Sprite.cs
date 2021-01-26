using Bearded.TD.Content.Models;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Loading
{
    class Sprite : ISprite
    {
        private readonly UVRectangle uv;

        public Vector2 BaseSize { get; }
        public UVRectangle UV => uv;

        public Sprite(UVRectangle uv, Vector2 baseSize)
        {
            this.uv = uv;
            BaseSize = baseSize;
        }
    }
}
