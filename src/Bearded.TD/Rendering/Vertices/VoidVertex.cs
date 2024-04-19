using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct VoidVertex(Vector3 position) : IVertexData
{
    private readonly Vector3 position = position;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = VertexData.MakeAttributeArray(VertexData.MakeAttributeTemplate<Vector3>("v_position")
        );
}
