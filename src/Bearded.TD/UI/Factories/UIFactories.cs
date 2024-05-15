using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Tooltips;

namespace Bearded.TD.UI.Factories;

sealed record UIFactories(
    ButtonFactory ButtonFactory,
    ReportFactory ReportFactory
)
{
    public static UIFactories Create(Animations animations, TooltipFactory tooltips)
    {
        return new UIFactories(
            new ButtonFactory(animations, tooltips),
            new ReportFactory(animations, tooltips)
        );
    }
}
