using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeVertex : IVertexData
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly Vector3 position;
    private readonly Color color;

    public ShapeVertex(Vector3 position, Color color)
    {
        this.position = position;
        this.color = color;
    }

    private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
        MakeAttributeTemplate<Vector3>("v_position"),
        MakeAttributeTemplate<Color>("v_color")
    );

    VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
}
