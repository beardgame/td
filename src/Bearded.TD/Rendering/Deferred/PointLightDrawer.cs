using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Deferred;

sealed class PointLightDrawer
{
    private readonly ShapeDrawer3<PointLightVertex, (Vector3, float, Color, float, bool)> drawer;

    public PointLightDrawer(IIndexedTrianglesMeshBuilder<PointLightVertex, ushort> meshBuilder)
    {
        drawer = new ShapeDrawer3<
            PointLightVertex,
            (Vector3 Center, float RadiusSquared, Color Color, float Intensity, bool drawShadow)
            >(
            meshBuilder, (xyz, p) =>
                new PointLightVertex(xyz, p.Center, p.RadiusSquared, p.Color, p.Intensity, (byte)(p.drawShadow ? 1 : 0))
            );
    }

    public void Draw(
        Vector3 center,
        float radius,
        Color color,
        float intensity = 1,
        bool drawShadow = true)
    {
        // 1f / sqrt((5 + 2 * sqrt(5)) / 15)
        const float innerToOuterRadius = 1f / 0.794654472f;

        var rSquared = radius.Squared();
        var parameters = (center, rSquared, color, intensity, drawShadow);

        // TODO: confirm this draws at the right scale so that radius is the inner radius of the icosahedron
        drawer.DrawIcosahedron(center, radius * innerToOuterRadius, parameters);
    }
}
