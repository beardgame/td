﻿using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

using static Bearded.TD.Constants.UI.Button;

namespace Bearded.TD.Rendering.UI;

sealed class ButtonBackgroundEffectRenderer(IShapeDrawer drawer) : IRenderer<ButtonBackgroundEffect>
{
    public void Render(ButtonBackgroundEffect control)
    {
        if (!control.MouseIsOver || !control.ButtonIsEnabled)
            return;

        if (control.Parent is Button {IsEnabled: false})
            return;

        var frame = control.Frame;

        var color = control.MouseIsDown ? ActiveColor : HoverColor;

        var shape = Shapes.Shapes.Rectangle(frame.TopLeft, frame.Size);
        var components = new ShapeComponents(Fill: color).ForDrawingAssumingNoGradients();

        drawer.Draw(shape, components);
    }
}
