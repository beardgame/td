﻿using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using Bearded.Graphics.Shapes;
using Bearded.Graphics.Text;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Loading;
using Bearded.TD.Rendering.Vertices;
using OpenTK.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering
{
    sealed class CoreDrawers
    {
        public IShapeDrawer2<Color> Primitives { get; }
        public IDrawableSprite<Color> CustomPrimitives { get; }

        public TextDrawerWithDefaults<Color> ConsoleFont { get; }
        public TextDrawerWithDefaults<Color> InGameConsoleFont { get; }
        public IShapeDrawer2<Color> ConsoleBackground { get; }

        public TextDrawerWithDefaults<Color> UIFont { get; }

        public PointLightDrawer PointLight { get; }
        public SpotlightDrawer Spotlight { get; }

        public CoreDrawers(CoreRenderers renderers, DeferredRenderer deferredRenderer)
        {
            Primitives = new ShapeDrawer2<ColorVertexData, Color>(
                renderers.Primitives, (xyz, color) => new ColorVertexData(xyz, color));
            CustomPrimitives = new DrawableSprite<ColorVertexData, Color>(
                renderers.Primitives, (position, _, color) => new ColorVertexData(position, color),
                new SpriteParameters(default, Vector2.One));
            ConsoleBackground = new ShapeDrawer2<ColorVertexData, Color>(
                renderers.ConsoleBackground, (xyz, color) => new ColorVertexData(xyz, color));

            ConsoleFont = createTextDrawerWithDefaults(renderers.ConsoleFont, renderers.ConsoleFontMeshBuilder);
            InGameConsoleFont = ConsoleFont.With(unitDownDP: -Vector3.UnitY);
            UIFont = createTextDrawerWithDefaults(renderers.UIFont, renderers.UIFontMeshBuilder);
            PointLight = new PointLightDrawer(deferredRenderer.PointLights);
            Spotlight = new SpotlightDrawer(deferredRenderer.Spotlights);
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
