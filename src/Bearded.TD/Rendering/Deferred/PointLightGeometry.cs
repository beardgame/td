using System;
using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.Utilities;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.Deferred
{
    class PointLightGeometry
    {
        private readonly ShapeDrawer3<PointLightVertex, (Vector3, float, Color)> drawer;

        public PointLightGeometry(IIndexedTrianglesMeshBuilder<PointLightVertex, ushort> meshBuilder)
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
