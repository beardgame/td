using amulware.Graphics;
using OpenTK.Mathematics;

namespace Bearded.TD.Content.Models
{
    interface ISprite
    {
        Vector2 BaseSize { get; }

        void Draw(Vector3 position, Color color, float size, float angle);

        void Draw(Vector3 position, Color color, float size);

        void DrawQuad(
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
            Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3,
            Color color0, Color color1, Color color2, Color color3);
    }
}
