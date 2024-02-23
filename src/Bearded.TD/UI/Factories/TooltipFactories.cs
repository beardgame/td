using System;
using System.Collections.Generic;
using Bearded.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Tooltips;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI.Tooltip;
using static Bearded.TD.UI.Factories.TextFactories;

namespace Bearded.TD.UI.Factories;

static class TooltipFactories
{
    public static Tooltip ShowSimpleTooltip(
        this TooltipFactory factory, string text, TooltipAnchor anchor, double? width = null)
    {
        return factory.ShowTooltip(SimpleTooltip(text, width), anchor);
    }

    public static Tooltip ShowSimpleTooltip(
        this TooltipFactory factory, ICollection<string> text, TooltipAnchor anchor, double? width = null)
    {
        return factory.ShowTooltip(SimpleTooltip(text, width), anchor);
    }

    public static TooltipDefinition SimpleTooltip(string text, double? width = null)
    {
        Control createControl() => tooltip(Label(text, Controls.Label.TextAnchorLeft));
        return new TooltipDefinition(createControl, width ?? DefaultWidth, Constants.UI.Text.LineHeight + 2 * Margin);
    }

    public static TooltipDefinition SimpleTooltip(ICollection<string> text, double? width = null)
    {
        Control createControl() => tooltip(l =>
        {
            foreach (var line in text)
            {
                l.AddLabel(line, Controls.Label.TextAnchorLeft);
            }
        });

        return new TooltipDefinition(
            createControl, width ?? DefaultWidth, text.Count * Constants.UI.Text.LineHeight + 2 * Margin);
    }

    private static Control tooltip(Action<Layouts.FixedColumnLayout> layoutBuilder)
    {
        var layoutControl = new CompositeControl();
        layoutBuilder(layoutControl.BuildFixedColumn());
        return tooltip(layoutControl);
    }

    private static Control tooltip(Control content)
    {
        var box = new ComplexBox
        {
            CornerRadius = 2,

            FillColor = Constants.UI.Colors.Get(BackgroundColor.Tooltip),

            EdgeOuterWidth = 1,
            EdgeColor = Constants.UI.Colors.Get(BackgroundColor.SubtleOutline),
        };
        return new CompositeControl
        {
            new BoxShadow
            {
                Color = Color.Black * 0.5f,
                Offset = new(0, 2, 0),
                BlurRadius = 5,
            }.WithCornerRadiusSource(box),
            box,
            content.Anchor(a => a.MarginAllSides(Margin)),
        };
    }
}
