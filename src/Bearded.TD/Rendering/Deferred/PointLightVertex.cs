using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred;

[StructLayout(LayoutKind.Sequential)]
readonly struct PointLightVertex : IVertexData
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly Vector3 vertexPosition;
    private readonly Vector3 vertexLightPosition;
    private readonly float vertexLightRadiusSquared;
    private readonly Color vertexLightColor;
    private readonly float intensity;

    public PointLightVertex(
        Vector3 vertexPosition,
        Vector3 vertexLightPosition,
        float vertexLightRadiusSquared,
        Color vertexLightColor,
        float intensity)
    {
        this.vertexPosition = vertexPosition;
        this.vertexLightPosition = vertexLightPosition;
        this.vertexLightRadiusSquared = vertexLightRadiusSquared;
        this.vertexLightColor = vertexLightColor;
        this.intensity = intensity;
    }

    private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
        MakeAttributeTemplate<Vector3>("vertexPosition"),
        MakeAttributeTemplate<Vector3>("vertexLightPosition"),
        MakeAttributeTemplate<float>("vertexLightRadiusSquared"),
        MakeAttributeTemplate<Color>("vertexLightColor"),
        MakeAttributeTemplate<float>("intensity")
    );

    VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
}
