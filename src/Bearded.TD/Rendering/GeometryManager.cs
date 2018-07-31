using amulware.Graphics;
using Bearded.TD.Rendering.Deferred;

namespace Bearded.TD.Rendering
{
    class GeometryManager
    {
        private readonly SurfaceManager surfaces;

        public PrimitiveGeometry Primitives { get; }

        public FontGeometry ConsoleFont { get; }
        public PrimitiveGeometry ConsoleBackground { get; }

        public FontGeometry UIFont { get; }

        public LevelGeometry Level { get; }
        
        public PointLightGeometry PointLight { get; }

        public GeometryManager(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            Primitives = new PrimitiveGeometry(surfaces.Primitives);
            ConsoleBackground = new PrimitiveGeometry(surfaces.ConsoleBackground);
            ConsoleFont = new FontGeometry(surfaces.ConsoleFontSurface, surfaces.ConsoleFont);
            UIFont = new FontGeometry(surfaces.UIFontSurface, surfaces.UIFont);
            Level = new LevelGeometry(surfaces.LevelSurface);
            PointLight = new PointLightGeometry(surfaces.PointLights);
        }
    }
}
