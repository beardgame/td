using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct DeferredSprite3DVertex : IVertexData
    {
        private readonly Vector3 position;
        private readonly Vector3 normal;
        private readonly Vector3 tangent;
        private readonly Vector2 uv;
        private readonly Color color;

        public DeferredSprite3DVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.tangent = tangent;
            this.uv = uv;
            this.color = color;
        }

        public VertexAttribute[] VertexAttributes => vertexAttributes;

        private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector3>("vertexTangent"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<Color>("vertexColor")
            );
    }
}
