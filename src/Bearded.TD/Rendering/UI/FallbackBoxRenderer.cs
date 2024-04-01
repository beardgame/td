using Bearded.Graphics;
using Bearded.TD.Meta;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;

namespace Bearded.TD.Rendering.UI;

sealed class FallbackBoxRenderer(IShapeDrawer drawer, IShapeComponentBuffer shapeComponents)
    : BoxRenderer(drawer, shapeComponents, Color.Fuchsia)
{
    public override void Render(Control control)
    {
        if (!UserSettings.Instance.Debug.RenderUIFallBack)
            return;

        base.Render(control);
    }
}
