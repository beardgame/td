using amulware.Graphics;
using OpenToolkit.Mathematics;
using static amulware.Graphics.VertexData;

namespace Bearded.TD.Rendering.Deferred
{
    struct PointLightVertex : IVertexData
    {
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

        public VertexAttribute[] VertexAttributes()
            => MakeAttributeArray(
                MakeAttributeTemplate<Vector3>("vertexPosition"),
                MakeAttributeTemplate<Vector3>("vertexLightPosition"),
                MakeAttributeTemplate<float>("vertexLightRadiusSquared"),
                MakeAttributeTemplate<Color>("vertexLightColor")
                );
    }
}
