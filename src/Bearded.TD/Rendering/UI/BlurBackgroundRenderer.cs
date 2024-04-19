using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class BlurBackgroundRenderer(CoreDrawers drawers) : IRenderer<BlurBackground>
{
    public void Render(BlurBackground control)
    {
        var frame = control.Frame;

        drawers.IntermediateLayerBlur.FillRectangle(
            (float)frame.X.Start,
            (float)frame.Y.Start,
            0,
            (float)frame.X.Size,
            (float)frame.Y.Size,
            default
        );
    }
}
