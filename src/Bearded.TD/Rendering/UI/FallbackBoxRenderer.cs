using Bearded.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;

namespace Bearded.TD.Rendering.UI;

sealed class FallbackBoxRenderer(IShapeDrawer drawer) : BoxRenderer(drawer, Color.Fuchsia)
{
    public override void Render(Control control)
    {
        if (!UserSettings.Instance.Debug.RenderUIFallBack)
            return;

        base.Render(control);
    }
}
