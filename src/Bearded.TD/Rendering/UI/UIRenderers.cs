using System;
using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Content.CoreUI;

namespace Bearded.TD.Rendering.UI;

sealed class UIRenderers(RenderContext context, ContentManager content, Blueprints coreBlueprints)
    : IRendererRouter
{
    private CachedRendererRouter router = null!;

    public void Render<T>(T control)
    {
        router.Render(control);
    }

    public void Reload()
    {
        var renderers = context.Renderers.DrawableRenderers;
        renderers.DisposeAll();

        var uiFonts = UIFonts.Load(coreBlueprints, renderers);

        context.Renderers.SetInGameConsoleFont(uiFonts.Default.With(unitDownDP: -Vector3.UnitY));

        var spriteShader = coreBlueprints.Shaders[DefaultShaders.Sprite];
        var shapeShader = coreBlueprints.Shaders[DefaultShaders.Shapes];

        var gradients = context.Renderers.Gradients;
        var shapeComponents = context.Renderers.ShapeComponents;
        var shapeDrawer = ShapeDrawer
            .GetOrCreate(renderers, gradients, shapeComponents, shapeShader, DrawOrderGroup.UIShapes, 0);

        router = new CachedRendererRouter(
        [
            validate(new UIDebugOverlayHighlightRenderer(context.Drawers.ConsoleBackground, uiFonts.Default)),
            validate(new RenderLayerCompositeControlRenderer(context.Compositor)),
            validate(new AutoCompletingTextInputRenderer(shapeDrawer, shapeComponents, uiFonts)),
            validate(new TextInputRenderer(shapeDrawer, shapeComponents, uiFonts)),
            validate(new LabelRenderer(uiFonts)),
            validate(new SpriteRenderer(content, renderers, spriteShader)),
            validate(new BorderRenderer(shapeDrawer, shapeComponents)),
            validate(new BackgroundBoxRenderer(shapeDrawer, shapeComponents)),
            validate(new ComplexShapeRenderer(shapeDrawer, shapeComponents, gradients)),
            validate(new DropShadowRenderer(shapeDrawer, shapeComponents)),
            validate(new DotRenderer(shapeDrawer, shapeComponents)),
            validate(new CaveBackgroundRenderer(content, renderers)),
            validate(new FallbackBoxRenderer(shapeDrawer, shapeComponents)),
        ]);
    }

    private static (Type ControlType, object Renderer) validate<TControl>(IRenderer<TControl> renderer)
        where TControl : Control
        => (typeof(TControl), renderer);
}
