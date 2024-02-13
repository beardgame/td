using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
readonly struct PointLightVertex(
    Vector3 vertexPosition,
    Vector3 vertexLightPosition,
    float vertexLightRadiusSquared,
    Color vertexLightColor,
    float intensity,
    byte shadow)
    : IVertexData
{
    private readonly Vector3 vertexPosition = vertexPosition;
    private readonly Vector3 vertexLightPosition = vertexLightPosition;
    private readonly float vertexLightRadiusSquared = vertexLightRadiusSquared;
    private readonly Color vertexLightColor = vertexLightColor;
    private readonly float intensity = intensity;
    private readonly byte shadow = shadow;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexLightPosition"),
            MakeAttributeTemplate<float>("vertexLightRadiusSquared"),
            MakeAttributeTemplate<Color>("vertexLightColor"),
            MakeAttributeTemplate<float>("intensity"),
            MakeAttributeTemplate<byte>("vertexShadow")
        );
}
