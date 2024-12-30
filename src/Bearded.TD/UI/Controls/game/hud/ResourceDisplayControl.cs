using System;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Rendering.UI;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Window;

namespace Bearded.TD.UI.Controls;

sealed class ResourceDisplayControl : CompositeControl
{
    public ResourceDisplayControl(ResourceDisplay model, UIContext context)
    {
        var coreEnergyDisplay = makeSingleResourceStack(
            context.Animations,
            model.CurrentCoreEnergy,
            model.CoreEnergyLeftThisWave,
            Constants.Content.CoreUI.Sprites.CoreEnergyIcon,
            Constants.Game.GameUI.EnergyColor, Constants.Game.GameUI.EnergyColorNegative
        );

        var scrapDisplay = makeSingleResourceStack(
            context.Animations,
            model.CurrentScrap,
            model.ScrapLeftThisWave,
            Constants.Content.CoreUI.Sprites.ScrapIcon,
            Constants.Game.GameUI.ResourcesColor, Constants.Game.GameUI.ResourcesColorNegative
        );

        var exchange = new CoreEnergyExchangeControl(model.Exchange, context);

        var singleResourceStackWidth = 80;

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
        Animations animations,
        IReadonlyBinding<Resource<T>> available,
        IReadonlyBinding<Resource<T>> leftThisWave,
        ModAwareSpriteId sprite,
        Color color, Color colorNegative)
        where T : IResourceType
    {
        var margin = 4;
        var size = Text.FontSize * 1.2;
        var height = Text.LineHeight;

        var resourceLabelConfig = new AnimatedNumberLabel.Configuration
        {
            MinChangeToAnimate = 0,
            MinAnimationTime = 0.2.S(),
            MaxAnimationTime = 1.S(),
            RenderZero = true,
            FlipScrollDirection = false,
            ShowPlusSign = false,
        };
        var leftThisWaveLabelConfig = new AnimatedNumberLabel.Configuration
        {
            MinChangeToAnimate = 0,
            MinAnimationTime = 0.2.S(),
            MaxAnimationTime = 1.S(),
            RenderZero = false,
            FlipScrollDirection = true,
            ShowPlusSign = true,
        };

        var resourceLabel = new AnimatedNumberLabel(
            resourceLabelConfig, animations, available.Transform(r => r.Value));
        TextFactories.SetDefaultLabelStyle(
            resourceLabel,
            fontSize: size,
            textAnchor: Label.TextAnchorRight,
            color: available.Transform(r => r.Value >= 0 ? color : colorNegative)
            );

        var leftThisWaveLabel = new AnimatedNumberLabel(
                leftThisWaveLabelConfig, animations, leftThisWave.Transform(r => r.Value));
        TextFactories.SetDefaultLabelStyle(
            leftThisWaveLabel,
            textAnchor: Label.TextAnchorRight,
            color: leftThisWave.Transform(r => r.Value >= 0 ? color : colorNegative)
        );

        var control = new CompositeControl
        {
            new Sprite { SpriteId = sprite, Color = color }
                .Anchor(a => a.Left(margin, size).Bottom(relativePercentage: 0.5, height: height)),

            resourceLabel.Anchor(a => a.Right(margin).Bottom(relativePercentage: 0.5, height: height)),

            leftThisWaveLabel.Anchor(a => a.Right(margin).Top(relativePercentage: 0.5, height: height)),
        };

        return control;
    }
}
