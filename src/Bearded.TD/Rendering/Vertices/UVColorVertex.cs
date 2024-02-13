using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct UVColorVertex(Vector3 position, Vector2 uv, Color color) : IVertexData
{
    private readonly Vector3 position = position;
    private readonly Vector2 uv = uv;
    private readonly Color color = color;

    public static CreateVertex<UVColorVertex, Color> Create { get; } =
        (p, uv, color) => new UVColorVertex(p, uv, color);

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("v_position"),
            MakeAttributeTemplate<Vector2>("v_texcoord"),
            MakeAttributeTemplate<Color>("v_color")
        );
}
