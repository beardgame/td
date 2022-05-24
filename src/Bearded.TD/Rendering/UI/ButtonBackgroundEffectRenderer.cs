using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

using static Bearded.TD.Constants.UI.Button;

namespace Bearded.TD.Rendering.UI;

sealed class ButtonBackgroundEffectRenderer : IRenderer<ButtonBackgroundEffect>
{
    private readonly IShapeDrawer2<Color> drawer;

    public ButtonBackgroundEffectRenderer(IShapeDrawer2<Color> drawer)
    {
        this.drawer = drawer;
    }

    public void Render(ButtonBackgroundEffect control)
    {
        if (!control.MouseIsOver || !control.ButtonIsEnabled)
            return;

        if (control.Parent is Button {IsEnabled: false})
            return;

        var frame = control.Frame;

        var color = control.MouseIsDown ? ActiveColor : HoverColor;

        drawer.FillRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, color);
    }
}
