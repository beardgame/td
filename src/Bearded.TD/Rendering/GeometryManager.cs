using System.Collections.Generic;
using System.Linq;
using amulware.Graphics;

namespace Bearded.TD.Rendering
{
    class GeometryManager
    {
        private readonly SurfaceManager surfaces;

        public FontGeometry ConsoleFont { get; }
        public PrimitiveGeometry ConsoleBackground { get; }
        
        public Dictionary<string, Sprite2DGeometry> Sprites { get; }

        public GeometryManager(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            ConsoleBackground = new PrimitiveGeometry(surfaces.ConsoleBackground);
            ConsoleFont = new FontGeometry(surfaces.ConsoleFontSurface, surfaces.ConsoleFont);
            
            Sprites = surfaces.GameSurfaces.Surfaces.ToDictionary(
                kvp => kvp.Key, kvp => new Sprite2DGeometry(kvp.Value)
                );
        }
    }
}
