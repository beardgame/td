using System.Runtime.InteropServices;
using Bearded.Graphics.Vertices;
using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Vertices;

[StructLayout(LayoutKind.Sequential)]
readonly struct HeightmapSplatVertex : IVertexData
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly Vector2 position;
    private readonly Vector2 uv;
    private readonly float minHeight;
    private readonly float maxHeight;

    public static DrawableSprite<HeightmapSplatVertex, (float, float)>.CreateSprite Create { get; } =
        (p, uv, minMax) => new HeightmapSplatVertex(p.Xy, uv, minMax.Item1, minMax.Item2);

    public HeightmapSplatVertex(Vector2 position, Vector2 uv, float minHeight, float maxHeight)
    {
        this.position = position;
        this.uv = uv;
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
    }

    public VertexAttribute[] VertexAttributes => vertexAttributes;

    private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
        MakeAttributeTemplate<Vector2>("vertexPosition"),
        MakeAttributeTemplate<Vector2>("vertexUV"),
        MakeAttributeTemplate<float>("vertexMinHeight"),
        MakeAttributeTemplate<float>("vertexMaxHeight")
    );
}