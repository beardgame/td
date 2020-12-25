using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.Rendering.Deferred;
using OpenTK.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering
{
    sealed class CoreDrawers
    {
        public IShapeDrawer2<Color> Primitives { get; }

        public TextDrawerWithDefaults<Color> ConsoleFont { get; }
        public TextDrawerWithDefaults<Color> InGameConsoleFont { get; }
        public IShapeDrawer2<Color> ConsoleBackground { get; }

        public TextDrawerWithDefaults<Color> UIFont { get; }

        public PointLightGeometry PointLight { get; }
        public SpotlightGeometry Spotlight { get; }

        public CoreDrawers(CoreRenderers renderers, DeferredRenderer deferredRenderer)
        {
            Primitives = new ShapeDrawer2<ColorVertexData, Color>(
                renderers.Primitives, (xyz, color) => new ColorVertexData(xyz, color));
            ConsoleBackground = new ShapeDrawer2<ColorVertexData, Color>(
                renderers.ConsoleBackground, (xyz, color) => new ColorVertexData(xyz, color));

            ConsoleFont = createTextDrawerWithDefaults(renderers.ConsoleFont, renderers.ConsoleFontMeshBuilder);
            InGameConsoleFont = ConsoleFont.With(unitDownDP: -Vector3.UnitY);
            UIFont = createTextDrawerWithDefaults(renderers.UIFont, renderers.UIFontMeshBuilder);
            PointLight = new PointLightGeometry(deferredRenderer.PointLights);
            Spotlight = new SpotlightGeometry(deferredRenderer.Spotlights);
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
