using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
readonly struct PointLightVertex(Vector3 position) : IVertexData
{
    private readonly Vector3 position = position;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(MakeAttributeTemplate<Vector3>("vertexPosition"));
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
readonly struct PointLightInstance(
    Vector3 center,
    float radius,
    Color color,
    float intensity,
    byte shadow)
    : IVertexData
{
    private readonly Vector3 center = center;
    private readonly float radius = radius;
    private readonly Color color = color;
    private readonly float intensity = intensity;
    private readonly byte shadow = shadow;

    static ImmutableArray<VertexAttribute> IVertexData.VertexAttributes { get; }
        = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("instanceLightPosition", instanced: true),
            MakeAttributeTemplate<float>("instanceLightRadius", instanced: true),
            MakeAttributeTemplate<Color>("instanceLightColor", instanced: true),
            MakeAttributeTemplate<float>("instanceIntensity", instanced: true),
            MakeAttributeTemplate<byte>("instanceShadow", instanced: true)
        );
}
