using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
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
        var coreEnergyDisplay = makeSingleResourceStack(
            model.CurrentCoreEnergy,
            model.CoreEnergyLeftThisWave,
            Constants.Content.CoreUI.Sprites.CoreEnergyIcon,
            Constants.Game.GameUI.EnergyColor
        );

        var scrapDisplay = makeSingleResourceStack(
            model.CurrentScrap,
            model.ScrapLeftThisWave,
            Constants.Content.CoreUI.Sprites.ScrapIcon,
            Constants.Game.GameUI.ResourcesColor
        );

        var exchange = new CoreEnergyExchangeControl(model.Exchange, context);

        var singleResourceStackWidth = 60;

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
            coreEnergyDisplay.Anchor(a => a.Left(width: singleResourceStackWidth)),
            scrapDisplay.Anchor(a => a.Right(width: singleResourceStackWidth)),
            exchange.Anchor(a => a.Left(singleResourceStackWidth).Right(singleResourceStackWidth)),
        };

        this.BuildLayout()
            .ForContentBox()
            .FillContent(content);
    }

    private static Control makeSingleResourceStack<T>(
        IReadonlyBinding<Resource<T>> available,
        IReadonlyBinding<Resource<T>> leftThisWave,
        ModAwareSpriteId sprite,
        Color color)
        where T : IResourceType
    {
        var margin = 4;
        var size = Text.FontSize * 1.2;
        var height = Text.LineHeight;

        var resourceLabel = TextFactories.Label(
            available.Transform(r => $"{(int)r.Value}"),
            Label.TextAnchorRight,
            Binding.Constant(color)
        );

        resourceLabel.FontSize = size;

        return new CompositeControl
        {
            new Sprite { SpriteId = sprite, Color = color }
                .Anchor(a => a.Left(margin, size).Bottom(relativePercentage: 0.5, height: height)),

            resourceLabel.Anchor(a => a.Right(margin).Bottom(relativePercentage: 0.5, height: height)),

            TextFactories.Label(
                leftThisWave.Transform(r => $"{(int)r.Value:+0;-#}"),
                Label.TextAnchorRight,
                Binding.Constant(color)
            ).Anchor(a => a.Right(margin).Top(relativePercentage: 0.5, height: height)),
        };
    }
}
