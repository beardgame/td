using Bearded.TD.Content;
using Bearded.TD.Content.Models;
using Bearded.TD.Game;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Content.CoreUI;
using TextInput = Bearded.TD.UI.Controls.TextInput;

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

        var shapeDrawer = ShapeDrawer.GetOrCreate(renderers, shapeShader, DrawOrderGroup.UIBackground, 0);

        router = new CachedRendererRouter(
        [
            (typeof(UIDebugOverlayControl.Highlight),
                new UIDebugOverlayHighlightRenderer(context.Drawers.ConsoleBackground, uiFonts.Default)),
            (typeof(RenderLayerCompositeControl),
                new RenderLayerCompositeControlRenderer(context.Compositor)),
            (typeof(AutoCompletingTextInput),
                new AutoCompletingTextInputRenderer(shapeDrawer, uiFonts.Default)),
            (typeof(TextInput), new TextInputRenderer(shapeDrawer, uiFonts.Default)),
            (typeof(Label), new LabelRenderer(uiFonts)),
            (typeof(Sprite), new SpriteRenderer(content, renderers, spriteShader)),
            (typeof(Border), new BorderRenderer(shapeDrawer)),
            (typeof(BackgroundBox), new BackgroundBoxRenderer(shapeDrawer)),
            (typeof(ComplexBox), new ComplexBoxRenderer(shapeDrawer)),
            (typeof(ButtonBackgroundEffect), new ButtonBackgroundEffectRenderer(shapeDrawer)),
            (typeof(Dot), new DotRenderer(shapeDrawer)),
            (typeof(Control), new FallbackBoxRenderer(shapeDrawer)),
        ]);
    }
}
