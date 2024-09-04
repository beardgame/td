using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Window;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplayControl : CompositeControl
{
    public ResourceDisplayControl(ResourceDisplay model, UIContext context)
    {
        var coreEnergyDisplay = new CompositeControl
        {
            TextFactories.Label(
                model.CurrentCoreEnergy.Transform(r => $"{(int)r.Value}"),
                Label.TextAnchorRight,
                Binding.Constant(Constants.Game.GameUI.EnergyColor)
            ).Anchor(a => a.MarginAllSides(4)),
        };
        var scrapDisplay = new CompositeControl
        {
            TextFactories.Label(
                model.CurrentScrap.Transform(r => $"{(int)r.Value}"),
                Label.TextAnchorRight,
                Binding.Constant(Constants.Game.GameUI.ResourcesColor)
            ).Anchor(a => a.MarginAllSides(4)),
        };

        var exchange = new CoreEnergyExchangeControl(model.Exchange, context);

        var content = new CompositeControl
        {
            new ComplexBox
            {
                CornerRadius = CornerRadius,
                Components = BackgroundComponents,
            }.WithDecorations(new Decorations(
                Shadow: Shadows.Popup,
                BlurredBackground: BlurredBackground.Default
            )),
            coreEnergyDisplay.Anchor(a => a.Left(width: 50)),
            scrapDisplay.Anchor(a => a.Right(width: 50)),
            exchange.Anchor(a => a.Left(50).Right(50)),
        };

        this.BuildLayout()
            .ForContentBox()
            .FillContent(content);
    }
}
