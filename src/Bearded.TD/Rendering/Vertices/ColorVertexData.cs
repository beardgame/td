using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct ColorVertexData : IVertexData
{
    public static ColorVertexData Create(Vector3 xyz, Color color) => new(xyz, color);

    private readonly Vector3 position;
    private readonly Color color;

    public static ImmutableArray<VertexAttribute> VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("v_position"),
            MakeAttributeTemplate<Color>("v_color")
        );

    public ColorVertexData(Vector3 position, Color color)
    {
        this.position = position;
        this.color = color;
    }

    public override string ToString() => $"{position}, {color}";
}
