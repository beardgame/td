using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred
{
    class PointLightDrawer
    {
        private readonly ShapeDrawer3<PointLightVertex, (Vector3, float, Color)> drawer;

        public PointLightDrawer(IIndexedTrianglesMeshBuilder<PointLightVertex, ushort> meshBuilder)
        {
            drawer = new ShapeDrawer3<PointLightVertex, (Vector3 Center, float RadiusSquared, Color Color)>(
                meshBuilder, (xyz, p) => new PointLightVertex(xyz, p.Center, p.RadiusSquared, p.Color));
        }

        public void Draw(Vector3 center, float radius, Color color)
        {
            // 1f / sqrt((5 + 2 * sqrt(5)) / 15)
            const float innerToOuterRadius = 1f / 0.794654472f;

            var rSquared = radius.Squared();
            var parameters = (center, rSquared, color);

            // TODO: confirm this draws at the right scale so that radius is the inner radius of the icosahedron
            drawer.DrawIcosahedron(center, radius * innerToOuterRadius, parameters);
        }
    }
}
