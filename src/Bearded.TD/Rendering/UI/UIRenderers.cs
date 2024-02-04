using Bearded.TD.Content;
using Bearded.TD.Game;
using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Layers;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;
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

        var drawers = context.Drawers;

        var uiFonts = UIFonts.Load(coreBlueprints, renderers);

        context.Renderers.SetInGameConsoleFont(uiFonts.Default.With(unitDownDP: -Vector3.UnitY));

        var spriteShader = coreBlueprints.Shaders[Constants.Content.CoreUI.DefaultShaders.Sprite];

        router = new CachedRendererRouter(
        [
            (typeof(UIDebugOverlayControl.Highlight),
                new UIDebugOverlayHighlightRenderer(drawers.ConsoleBackground, uiFonts.Default)),
            (typeof(RenderLayerCompositeControl),
                new RenderLayerCompositeControlRenderer(context.Compositor)),
            (typeof(AutoCompletingTextInput),
                new AutoCompletingTextInputRenderer(drawers.ConsoleBackground, uiFonts.Default)),
            (typeof(TextInput), new TextInputRenderer(drawers.ConsoleBackground, uiFonts.Default)),
            (typeof(Label), new LabelRenderer(uiFonts)),
            (typeof(Sprite), new SpriteRenderer(content, renderers, spriteShader)),
            (typeof(Border), new BorderRenderer(drawers.ConsoleBackground)),
            (typeof(BackgroundBox), new BackgroundBoxRenderer(drawers.ConsoleBackground)),
            (typeof(ButtonBackgroundEffect), new ButtonBackgroundEffectRenderer(drawers.ConsoleBackground)),
            (typeof(Dot), new DotRenderer(drawers.ConsoleBackground)),
            (typeof(Control), new FallbackBoxRenderer(drawers.ConsoleBackground)),
        ]);
    }
}
