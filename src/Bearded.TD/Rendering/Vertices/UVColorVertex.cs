using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Vertices
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct UVColorVertex : IVertexData
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly Vector3 position;
        private readonly Vector2 uv;
        private readonly Color color;

        public static DrawableSprite<UVColorVertex, Color>.CreateSprite Create { get; } =
            (p, uv, color) => new UVColorVertex(p, uv, color);

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
