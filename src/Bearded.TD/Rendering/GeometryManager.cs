using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.TD.Rendering.Deferred;

namespace Bearded.TD.Rendering
{
    class GeometryManager
    {
        private readonly SurfaceManager surfaces;

        public ColorShapeDrawer2 Primitives { get; }

        public FontGeometry ConsoleFont { get; }
        public PrimitiveGeometry ConsoleBackground { get; }

        public FontGeometry UIFont { get; }

        public PointLightGeometry PointLight { get; }
        public SpotlightGeometry Spotlight { get; }

        public GeometryManager(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            Primitives = new ColorShapeDrawer2(surfaces.Primitives);
            ConsoleBackground = new PrimitiveGeometry(surfaces.ConsoleBackground);
            ConsoleFont = new FontGeometry(surfaces.ConsoleFontSurface, surfaces.ConsoleFont);
            UIFont = new FontGeometry(surfaces.UIFontSurface, surfaces.UIFont);
            PointLight = new PointLightGeometry(surfaces.PointLights);
            Spotlight = new SpotlightGeometry(surfaces.Spotlights);
        }
    }
}
