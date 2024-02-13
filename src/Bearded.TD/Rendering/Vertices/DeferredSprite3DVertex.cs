using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct DeferredSprite3DVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv, Color color)
    : IVertexData
{
    public static CreateVertex<DeferredSprite3DVertex, (Vector3 Normal, Vector3 Tangent, Color Color)> Create { get; } =
        (position, uv, data) => new DeferredSprite3DVertex(
            position, data.Normal, data.Tangent, uv, data.Color);

    private readonly Vector3 position = position;
    private readonly Vector3 normal = normal;
    private readonly Vector3 tangent = tangent;
    private readonly Vector2 uv = uv;
    private readonly Color color = color;

    public static ImmutableArray<VertexAttribute> VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("v_position"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector3>("vertexTangent"),
            MakeAttributeTemplate<Vector2>("v_texcoord"),
            MakeAttributeTemplate<Color>("v_color")
        );
}
