using System.Runtime.InteropServices;
using amulware.Graphics.Vertices;
using OpenTK.Mathematics;
using static amulware.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct HeightmapSplatVertex : IVertexData
    {
        private readonly Vector2 position;
        private readonly Vector2 uv;
        private readonly float minHeight;
        private readonly float maxHeight;

        public HeightmapSplatVertex(Vector2 position, Vector2 uv, float minHeight, float maxHeight)
        {
            this.position = position;
            this.uv = uv;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
        }

        public VertexAttribute[] VertexAttributes => vertexAttributes;

        private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
            MakeAttributeTemplate<Vector2>("vertexPosition"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<float>("vertexMinHeight"),
            MakeAttributeTemplate<float>("vertexMaxHeight")
        );
    }
}
