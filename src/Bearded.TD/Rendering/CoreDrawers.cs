using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.Graphics.Text;
using Bearded.TD.Content.Models;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Rendering.Loading;
using OpenTK.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering;

sealed class CoreDrawers
{
    private readonly CoreRenderers renderers;

    public IShapeDrawer2<Color> Primitives { get; }
    public IDrawableSprite<Color> CustomPrimitives { get; }
    public IShapeDrawer2<Color> ConsoleBackground { get; }

    public TextDrawerWithDefaults<Color> InGameConsoleFont => renderers.InGameConsoleFont;

    public PointLightDrawer PointLight { get; }
    public SpotlightDrawer Spotlight { get; }

    public CoreDrawers(CoreRenderers renderers, DeferredRenderer deferredRenderer)
    {
        this.renderers = renderers;
        Primitives = new ShapeDrawer2<ColorVertexData, Color>(
            renderers.Primitives, (xyz, color) => new ColorVertexData(xyz, color));
        CustomPrimitives = new DrawableSprite<ColorVertexData, Color>(
            renderers.Primitives, (position, _, color) => new ColorVertexData(position, color),
            new SpriteParameters(default, Vector2.One));
        ConsoleBackground = new ShapeDrawer2<ColorVertexData, Color>(
            renderers.ConsoleBackground, (xyz, color) => new ColorVertexData(xyz, color));

        PointLight = new PointLightDrawer(deferredRenderer.PointLights);
        Spotlight = new SpotlightDrawer(deferredRenderer.Spotlights);
    }
}
