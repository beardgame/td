using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Rendering
{
    class GeometryManager
    {
        private readonly SurfaceManager surfaces;

        public PrimitiveGeometry Primitives { get; }

        public FontGeometry ConsoleFont { get; }
        public PrimitiveGeometry ConsoleBackground { get; }

        public FontGeometry UIFont { get; }
        
        public Dictionary<string, Sprite2DGeometry> Sprites { get; }

        public GeometryManager(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            Primitives = new PrimitiveGeometry(surfaces.Primitives);
            ConsoleBackground = new PrimitiveGeometry(surfaces.ConsoleBackground);
            ConsoleFont = new FontGeometry(surfaces.ConsoleFontSurface, surfaces.ConsoleFont);
            UIFont = new FontGeometry(surfaces.UIFontSurface, surfaces.UIFont);
            
            Sprites = surfaces.GameSurfaces.Surfaces.ToDictionary(
                kvp => kvp.Key, kvp => new Sprite2DGeometry(kvp.Value)
                );
        }
    }
}
