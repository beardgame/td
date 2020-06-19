using amulware.Graphics;
using Bearded.Utilities;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.Deferred
{
    class SpotlightGeometry
    {
        private readonly IndexedSurface<SpotlightVertex> surface;

        public SpotlightGeometry(IndexedSurface<SpotlightVertex> surface)
        {
            this.surface = surface;
        }

        public void Draw(Vector3 center, Vector3 direction, float radius, float angle, Color color)
        {
            var x0 = center.X - radius;
            var x1 = center.X + radius;
            var y0 = center.Y - radius;
            var y1 = center.Y + radius;

            var rSquared = radius.Squared();

            // TODO: account for perspective to prevent light cutoff
            // TODO: optimise to draw triangle/cone only
            surface.AddQuad(
                new SpotlightVertex(new Vector3(x0, y0, 0), center, direction, angle, rSquared, color),
                new SpotlightVertex(new Vector3(x1, y0, 0), center, direction, angle, rSquared, color),
                new SpotlightVertex(new Vector3(x1, y1, 0), center, direction, angle, rSquared, color),
                new SpotlightVertex(new Vector3(x0, y1, 0), center, direction, angle, rSquared, color)
            );
        }
    }
}
