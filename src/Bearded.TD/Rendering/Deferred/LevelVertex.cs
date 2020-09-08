using System.Runtime.InteropServices;
using amulware.Graphics;
using amulware.Graphics.Vertices;
using OpenToolkit.Mathematics;
using static amulware.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct LevelVertex : IVertexData
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

        private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<Color>("vertexColor")
        );

        VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
    }
}
