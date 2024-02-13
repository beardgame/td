using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct HeightmapSplatVertex(Vector2 position, Vector2 uv, float minHeight, float maxHeight)
    : IVertexData
{
    private readonly Vector2 position = position;
    private readonly Vector2 uv = uv;
    private readonly float minHeight = minHeight;
    private readonly float maxHeight = maxHeight;

    public static CreateVertex<HeightmapSplatVertex, (float, float)> Create { get; } =
        (p, uv, minMax) => new HeightmapSplatVertex(p.Xy, uv, minMax.Item1, minMax.Item2);

    public static ImmutableArray<VertexAttribute> VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector2>("vertexPosition"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<float>("vertexMinHeight"),
            MakeAttributeTemplate<float>("vertexMaxHeight")
        );
}
