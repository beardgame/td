﻿using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class BackgroundBoxRenderer(IShapeDrawer drawer, IShapeComponentBuffer shapeComponents) : IRenderer<BackgroundBox>
{
    public void Render(BackgroundBox control)
    {
        var color = control.Color;
        if (color == Color.Transparent)
            return;

        var frame = control.Frame;

        var shape = Rectangle(frame.TopLeft, frame.Size);
        var components = ShapeComponentsForDrawing.From(Fill.With(color), shapeComponents);

        drawer.Draw(shape, components);
    }
}
