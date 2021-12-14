using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.Meta;
using Bearded.UI.Controls;

namespace Bearded.TD.Rendering.UI;

sealed class FallbackBoxRenderer : BoxRenderer
{
    public FallbackBoxRenderer(IShapeDrawer2<Color> drawer)
        : base(drawer, Color.Fuchsia)
    {
    }

    public override void Render(Control control)
    {
        if (!UserSettings.Instance.Debug.RenderUIFallBack)
            return;

        base.Render(control);
    }
}