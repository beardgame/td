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
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly Vector2 UV = uv;
    public readonly Color Color = color;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexNormal"),
            MakeAttributeTemplate<Vector2>("vertexUV"),
            MakeAttributeTemplate<Color>("vertexColor")
        );
}
