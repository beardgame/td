using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Tooltips;

namespace Bearded.TD.UI.Factories;

sealed record UIFactories(
    ButtonFactory ButtonFactory,
    ReportFactory ReportFactory,
    SliderFactory SliderFactory
)
{
    public static UIFactories Create(Animations animations, TooltipFactory tooltips)
    {
        var buttons = new ButtonFactory(animations, tooltips);

        return new UIFactories(
            buttons,
            new ReportFactory(animations, tooltips),
            new SliderFactory(buttons, animations)
        );
    }
}
