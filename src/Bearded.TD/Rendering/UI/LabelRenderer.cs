﻿using Bearded.TD.UI;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI;

sealed class LabelRenderer(UIFonts uiFonts) : IRenderer<Label>
{
    public void Render(Label label)
    {
        var argb = label.Color;

        if (label.Parent is Button { IsEnabled: false })
        {
            argb *= 0.5f;
        }

        var textAnchor = label.TextAnchor;
        var frame = label.Frame;
        var anchor = frame.TopLeft + frame.Size * textAnchor;

        uiFonts.ForStyle(label.TextStyle).DrawLine(
            xyz: ((Vector2) anchor).WithZ(),
            text: label.Text,
            alignHorizontal: (float) textAnchor.X,
            alignVertical: (float) textAnchor.Y,
            fontHeight: (float) label.FontSize,
            parameters: argb
        );
    }
}
