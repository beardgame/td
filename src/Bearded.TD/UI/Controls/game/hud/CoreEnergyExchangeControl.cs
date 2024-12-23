using System;
using Bearded.Graphics;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using OpenTK.Mathematics;
using static Bearded.TD.Constants.Game.GameUI;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class CoreEnergyExchangeControl : CompositeControl
{
    public CoreEnergyExchangeControl(CoreEnergyExchange model, UIContext context)
    {
        GradientStop[] sliderBackgroundGradientStops =
        [
            (0, EnergyColor),
            (0.75, EnergyColor),
            (0.75, EnergyColorNegative),
            (0.75, EnergyColorNegative),
            (0.75, ResourcesColor),
            (1, ResourcesColor),
        ];

        ReadOnlySpan<ShapeComponent> sliderBackgroundComponents =
        [
            Fill.With(
                ShapeColor.FromMutable(
                    sliderBackgroundGradientStops,
                    GradientDefinition.Linear(
                        AnchorPoint.Relative((0, 0)),
                        AnchorPoint.Relative((1, 0))
                    )
                )),
            Edge.Outer(1, Colors.Get(ForeGroundColor.Edge)),
        ];

        var sliderBackground = new ComplexBox
        {
            Components = ShapeComponents.From(sliderBackgroundComponents),
            CornerRadius = 2,
        };

        var (range, stepSize) = model.ValidExchangePercentages;
        IReadonlyBinding<Vector2d> animatedExchangeRate = null!;
        var slider = context.Factories.SliderFactory.Create(b => b
            .WithHorizontalValue(model.ExchangePercentage, range, stepSize)
            .WithoutHandle()
            .WithBackground(Background.From(sliderBackground))
            .ReadAnimatedPercentage(out animatedExchangeRate)
        );

        animatedExchangeRate.ControlUpdated += onSliderAnimate;
        onSliderAnimate(animatedExchangeRate.Value);
        model.MaximumExchangePercentage.SourceUpdated += _ => updateBackgroundGradient();

        var percentageLabel = TextFactories.Label(
            text: model.ExchangePercentage.Transform(r => $"{(int)(r * 100):0}%"),
            color: Binding.Constant(Color.Black)
        );
        percentageLabel.IsClickThrough = true;

        var rateRow = makeRateRow(model);

        var percentageRow = new CompositeControl
        {
            slider.Anchor(a => a.Left(4).Right(4)),
            percentageLabel,
        };

        var background = new ComplexBox
        {
            Components = Window.BackgroundComponents,
            CornerRadius = Window.CornerRadius,
        };

        this.Add(
            [
                background,
                rateRow.Anchor(a => a
                    .Bottom(relativePercentage: 0.5, height: Text.LineHeight)
                    .Left(8)
                ),
                percentageRow.Anchor(a => a.Top(relativePercentage: 0.5, height: Text.LineHeight)),
            ]
        );

        return;

        var sliderSecondStop = 0.0;

        void onSliderAnimate(Vector2d p)
        {
            sliderSecondStop = p.X;
            updateBackgroundGradient();
        }

        void updateBackgroundGradient()
        {
            var maxValue = model.MaximumExchangePercentage.Value;
            var maxPercentage = (maxValue - range.Start) / range.Size;

            var firstStop = Math.Min(sliderSecondStop, maxPercentage);

            sliderBackgroundGradientStops[1] = (firstStop, EnergyColor);
            sliderBackgroundGradientStops[2] = (firstStop, EnergyColorNegative);
            sliderBackgroundGradientStops[3] = (sliderSecondStop, EnergyColorNegative);
            sliderBackgroundGradientStops[4] = (sliderSecondStop, ResourcesColor);
        }
    }

    private static CompositeControl makeRateRow(CoreEnergyExchange model)
    {
        const int energyConversionScale = 10;

        const int margin = 6;
        const int labelWidth = 20;
        const float size = Text.FontSize;

        return
        [
            new Sprite { SpriteId = Constants.Content.CoreUI.Sprites.CoreEnergyIcon, Color = EnergyColor }
                .Anchor(a => a
                    .Right(relativePercentage: 0.5, margin: labelWidth + margin * 2, width: size)
                ),

            TextFactories.Label(
                text: $"{energyConversionScale:0}",
                color: EnergyColor,
                textAnchor: Label.TextAnchorRight
            ).Anchor(a => a.Right(relativePercentage: 0.5, margin: margin, width: labelWidth)),

            TextFactories.Label(">"),
            new Sprite { SpriteId = Constants.Content.CoreUI.Sprites.ScrapIcon, Color = ResourcesColor }
                .Anchor(a => a.Left(relativePercentage: 0.5, margin: margin, width: size)),

            TextFactories.Label(
                text: model.CoreEnergyToScrapRate.Transform(r => $"{r.Value * energyConversionScale:0.0}"),
                color: Binding.Constant(ResourcesColor),
                textAnchor: Label.TextAnchorLeft
            ).Anchor(a => a.Left(relativePercentage: 0.5, margin: size + margin * 2, width: labelWidth)),

        ];
    }
}
