using System.Runtime.InteropServices;
using Bearded.Graphics;
using Bearded.Graphics.Vertices;
using OpenTK.Mathematics;
using static Bearded.Graphics.Vertices.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct PointLightVertex : IVertexData
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly Vector3 vertexPosition;
        private readonly Vector3 vertexLightPosition;
        private readonly float vertexLightRadiusSquared;
        private readonly Color vertexLightColor;

        public PointLightVertex(
            Vector3 vertexPosition,
            Vector3 vertexLightPosition,
            float vertexLightRadiusSquared,
            Color vertexLightColor)
        {
            this.vertexPosition = vertexPosition;
            this.vertexLightPosition = vertexLightPosition;
            this.vertexLightRadiusSquared = vertexLightRadiusSquared;
            this.vertexLightColor = vertexLightColor;
        }

        private static readonly VertexAttribute[] vertexAttributes = MakeAttributeArray(
            MakeAttributeTemplate<Vector3>("vertexPosition"),
            MakeAttributeTemplate<Vector3>("vertexLightPosition"),
            MakeAttributeTemplate<float>("vertexLightRadiusSquared"),
            MakeAttributeTemplate<Color>("vertexLightColor")
        );

        VertexAttribute[] IVertexData.VertexAttributes => vertexAttributes;
    }
}
