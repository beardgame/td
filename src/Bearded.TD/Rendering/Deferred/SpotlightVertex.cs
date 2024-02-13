using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred;

[StructLayout(LayoutKind.Sequential)]
readonly struct SpotlightVertex(
    Vector3 vertexPosition,
    Vector3 vertexLightPosition,
    Vector3 vertexLightDirection,
    float vertexLightAngle,
    float vertexLightRadiusSquared,
    Color vertexLightColor)
    : IVertexData
{
    private readonly Vector3 vertexPosition = vertexPosition;
    private readonly Vector3 vertexLightPosition = vertexLightPosition;
    private readonly Vector3 vertexLightDirection = vertexLightDirection;
    private readonly float vertexLightAngle = vertexLightAngle;
    private readonly float vertexLightRadiusSquared = vertexLightRadiusSquared;
    private readonly Color vertexLightColor = vertexLightColor;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexLightPosition"),
            MakeAttributeTemplate<Vector3>("vertexLightDirection"),
            MakeAttributeTemplate<float>("vertexLightAngle"),
            MakeAttributeTemplate<float>("vertexLightRadiusSquared"),
            MakeAttributeTemplate<Color>("vertexLightColor")
        );
}
