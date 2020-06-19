using amulware.Graphics;
using OpenToolkit.Mathematics;
using static amulware.Graphics.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    struct LevelVertex : IVertexData
    {
        private readonly Vector3 position;
        private readonly Vector3 normal;
        private readonly Vector2 uv;
        private readonly Color color;

        public LevelVertex(Vector3 position, Vector3 normal, Vector2 uv, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.color = color;
        }

        public VertexAttribute[] VertexAttributes()
            => MakeAttributeArray(
                MakeAttributeTemplate<Vector3>("vertexPosition"),
                MakeAttributeTemplate<Vector3>("vertexNormal"),
                MakeAttributeTemplate<Vector2>("vertexUV"),
                MakeAttributeTemplate<Color>("vertexColor")
            );
    }
}
