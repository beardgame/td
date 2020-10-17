using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.Rendering.Deferred;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering
{
    sealed class GeometryManager
    {
        private readonly SurfaceManager surfaces;

        public ColorShapeDrawer2 Primitives { get; }

        public TextDrawerWithDefaults<Color> ConsoleFont { get; }
        public PrimitiveGeometry ConsoleBackground { get; }

        public TextDrawerWithDefaults<Color> UIFont { get; }

        public PointLightGeometry PointLight { get; }
        public SpotlightGeometry Spotlight { get; }

        public GeometryManager(SurfaceManager surfaces)
        {
            this.surfaces = surfaces;

            Primitives = new ColorShapeDrawer2(surfaces.Primitives);
            ConsoleBackground = new PrimitiveGeometry(surfaces.ConsoleBackground);

            ConsoleFont = createTextDrawerWithDefaults(surfaces.ConsoleFont, surfaces.ConsoleFontMeshBuilder);
            UIFont = createTextDrawerWithDefaults(surfaces.UIFont, surfaces.UIFontMeshBuilder);
            PointLight = new PointLightGeometry(surfaces.PointLights);
            Spotlight = new SpotlightGeometry(surfaces.Spotlights);
        }

        private static TextDrawerWithDefaults<Color> createTextDrawerWithDefaults(
            Font consoleFont, IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder)
        {
            return createTextDrawer(consoleFont, meshBuilder)
                .WithDefaults(Constants.UI.Text.FontSize, 0, 0, Vector3.UnitX, Vector3.UnitY, Color.White);
        }

        private static ITextDrawer<Color> createTextDrawer(
            Font consoleFont, IIndexedTrianglesMeshBuilder<UVColorVertex, ushort> meshBuilder)
        {
            return new TextDrawer<UVColorVertex, Color>(
                consoleFont, meshBuilder, (xyz, uv, color) => new UVColorVertex(xyz, uv, color));
        }
    }
}
