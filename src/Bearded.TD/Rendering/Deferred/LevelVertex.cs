using amulware.Graphics;
using OpenTK;

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
            => VertexData.MakeAttributeArray(
                VertexData.MakeAttributeTemplate<Vector3>("vertexPosition"),
                VertexData.MakeAttributeTemplate<Vector3>("vertexNormal"),
                VertexData.MakeAttributeTemplate<Vector2>("vertexUV"),
                VertexData.MakeAttributeTemplate<Color>("vertexColor")
            );

        public int Size() => VertexData.SizeOf<LevelVertex>();
    }
}