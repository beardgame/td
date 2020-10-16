using amulware.Graphics;
using Bearded.Utilities;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.Deferred
{
    class PointLightGeometry
    {
        private readonly IndexedSurface<PointLightVertex> surface;

        public PointLightGeometry(IndexedSurface<PointLightVertex> surface)
        {
            this.surface = surface;
        }

        public void Draw(Vector3 center, float radius, Color color)
        {
            var x0 = center.X - radius;
            var x1 = center.X + radius;
            var y0 = center.Y - radius;
            var y1 = center.Y + radius;

            var rSquared = radius.Squared();

            // TODO: account for perspective to prevent light cutoff
            // OMG - we can use the ColorShapeDrawer3 if we just make it generic in the vertex!
            surface.AddQuad(
                new PointLightVertex(new Vector3(x0, y0, 0), center, rSquared, color),
                new PointLightVertex(new Vector3(x1, y0, 0), center, rSquared, color),
                new PointLightVertex(new Vector3(x1, y1, 0), center, rSquared, color),
                new PointLightVertex(new Vector3(x0, y1, 0), center, rSquared, color)
            );
        }
    }
}
