using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred;

[StructLayout(LayoutKind.Sequential)]
readonly struct LevelVertex(Vector3 position, Vector3 normal, Vector2 uv, Color color) : IVertexData
{
    private readonly Vector3 position = position;
    private readonly Vector3 normal = normal;
    private readonly Vector2 uv = uv;
    private readonly Color color = color;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<Color>("vertexColor")
        );
}
