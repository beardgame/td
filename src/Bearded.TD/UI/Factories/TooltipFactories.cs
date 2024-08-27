using System;
using System.Collections.Generic;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Tooltip;
using static Bearded.TD.UI.Factories.TextFactories;
using Tooltip = Bearded.TD.UI.Tooltips.Tooltip;

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
        return new TooltipDefinition(createControl, width ?? DefaultWidth, Text.LineHeight + 2 * Margin);

        Control createControl() => TooltipWithContent(Label(text, Controls.Label.TextAnchorLeft));
    }

    public static TooltipDefinition SimpleTooltip(IReadonlyBinding<string> text, double? width = null)
    {
        return new TooltipDefinition(createControl, width ?? DefaultWidth, Text.LineHeight + 2 * Margin);

        Control createControl() =>
            TooltipWithContent(
                    Label(text, Controls.Label.TextAnchorLeft))
                .BindIsVisible(text.Transform(t => !string.IsNullOrWhiteSpace(t)));
    }

    public static TooltipDefinition SimpleTooltip(ICollection<string> text, double? width = null)
    {
        return new TooltipDefinition(
            createControl, width ?? DefaultWidth, text.Count * Text.LineHeight + 2 * Margin);

        Control createControl() => tooltip(l =>
        {
            foreach (var line in text)
            {
                l.AddLabel(line, Controls.Label.TextAnchorLeft);
            }
        });
    }

    private static Control tooltip(Action<Layouts.FixedColumnLayout> layoutBuilder)
    {
        var layoutControl = new CompositeControl();
        layoutBuilder(layoutControl.BuildFixedColumn());
        return TooltipWithContent(layoutControl);
    }

    public static Control TooltipWithContent(Control content)
    {
        return new CompositeControl
        {
            new ComplexBox
            {
                CornerRadius = 2,
                Components = Constants.UI.Tooltip.Background,
            }.WithDropShadow(Shadow, ShadowFade),
            content.Anchor(a => a.MarginAllSides(Margin)),
        };
    }
}
