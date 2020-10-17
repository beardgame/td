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
            var rSquared = radius.Squared();
            var parameters = (center, rSquared, color);

            // TODO: confirm this draws at the right scale so that radius is the inner radius of the icosahedron
            drawer.DrawIcosahedron(center, radius, parameters);
        }
    }
}
