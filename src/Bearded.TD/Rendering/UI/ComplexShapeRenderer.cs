﻿using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class ComplexShapeRenderer(IShapeDrawer shapes, GradientDrawer gradients) : IRenderer<ComplexShapeControl>
{
    public void Render(ComplexShapeControl control)
    {
        var frame = control.Frame;
        var components = control.Components.ForDrawingWith(gradients);

        switch (control)
        {
            case ComplexBox:
            {
                shapes.Draw(Rectangle(frame.TopLeft, frame.Size, control.CornerRadius), components);
                break;
            }
            case ComplexCircle:
            {
                var center = frame.TopLeft + frame.Size * 0.5;
                var radius = Min(frame.Size.X, frame.Size.Y) * 0.5;

                shapes.Draw(Circle(center, radius), components);
                break;
            }
        }
    }
}
