using System;
using System.Collections.Generic;
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
        Control contentFactory() => tooltip(Label(text, Controls.Label.TextAnchorLeft));
        return factory.ShowTooltip(
            contentFactory, anchor, width ?? DefaultWidth, Constants.UI.Text.LineHeight + 2 * Margin);
    }

    public static Tooltip ShowSimpleTooltip(
        this TooltipFactory factory, ICollection<string> text, TooltipAnchor anchor, double? width = null)
    {
        Control contentFactory() => tooltip(l =>
        {
            foreach (var line in text)
            {
                l.AddLabel(line, Controls.Label.TextAnchorLeft);
            }
        });

        return factory.ShowTooltip(
            contentFactory, anchor, width ?? DefaultWidth, text.Count * Constants.UI.Text.LineHeight + 2 * Margin);
    }

    private static Control tooltip(Action<Layouts.FixedColumnLayout> layoutBuilder)
    {
        var layoutControl = new CompositeControl();
        layoutBuilder(layoutControl.BuildFixedColumn());
        return tooltip(layoutControl);
    }

    private static Control tooltip(Control content)
    {
        return new BackgroundBox { content.Anchor(a => a.MarginAllSides(Margin)) };
    }
}
