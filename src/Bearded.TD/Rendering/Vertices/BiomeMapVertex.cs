using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct BiomeMapVertex(Vector2 position, byte biomeId) : IVertexData
{
    private readonly Vector2 position = position;
    private readonly uint biomeId = biomeId;

    public static ImmutableArray<VertexAttribute> VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector2>("vertexPosition"),
            MakeAttributeTemplate<uint>("biomeId")
        );
}
