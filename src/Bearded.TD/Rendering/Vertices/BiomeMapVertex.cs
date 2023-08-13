using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct BiomeMapVertex : IVertexData
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly Vector2 position;
    private readonly uint biomeId;

    public BiomeMapVertex(Vector2 position, byte biomeId)
    {
        this.position = position;
        this.biomeId = biomeId;
    }

    public VertexAttribute[] VertexAttributes => vertexAttributes;

    private static readonly VertexAttribute[] vertexAttributes = VertexData.MakeAttributeArray(
        VertexData.MakeAttributeTemplate<Vector2>("vertexPosition"),
        VertexData.MakeAttributeTemplate<uint>("biomeId")
    );
}
