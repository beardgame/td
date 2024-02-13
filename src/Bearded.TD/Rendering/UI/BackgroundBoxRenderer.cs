using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class BackgroundBoxRenderer(IShapeDrawer drawer) : IRenderer<BackgroundBox>
{
    public void Render(BackgroundBox control)
    {
        var frame = control.Frame;

        drawer.DrawRectangle(
            frame.TopLeft,
            frame.Size,
            control.Color);
    }
}
