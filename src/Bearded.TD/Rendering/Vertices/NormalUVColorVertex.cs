using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct NormalUVVertex(Vector3 position, Vector3 normal, Vector2 uv) : IVertexData
{
    public override string ToString() => $"{position}, {normal}, {uv}";

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector2>("vertexUV")
        );
}
