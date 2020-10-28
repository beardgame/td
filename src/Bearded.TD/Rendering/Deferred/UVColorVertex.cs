using System.Runtime.InteropServices;
using amulware.Graphics;
using amulware.Graphics.Vertices;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.Deferred
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct UVColorVertex : IVertexData
    {
        private readonly Vector3 position;
        private readonly Vector2 uv;
        private readonly Color color;

        public UVColorVertex(Vector3 position, Vector2 uv, Color color)
        {
            this.position = position;
            this.uv = uv;
            this.color = color;
        }

        private static readonly VertexAttribute[] vertexAttributes = VertexData.MakeAttributeArray(
            VertexData.MakeAttributeTemplate<Vector3>("v_position"),
            VertexData.MakeAttributeTemplate<Vector2>("v_texcoord"),
            VertexData.MakeAttributeTemplate<Color>("v_color")
        );

        VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
    }
}
