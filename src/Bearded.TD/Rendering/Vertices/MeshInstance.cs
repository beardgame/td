using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
readonly struct MeshInstance(Matrix4 worldMatrix) : IVertexData
{
    // This could be a mat4x3 strictly speaking which would save upload bandwidth, but mat4 is easier to reason about
    // and is easy to premultiply with view & projection.
    private readonly Matrix4 worldMatrix = worldMatrix;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; } =
        MakeAttributeArray(
            MakeAttributeTemplate<Matrix4>("instanceMatrix", instanced: true)
        );
}
