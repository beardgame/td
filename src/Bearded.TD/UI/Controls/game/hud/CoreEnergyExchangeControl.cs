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
        var sliderBackgroundGradientStops = new GradientStop[]
        {
            (0, EnergyColor),
            (0.75, EnergyColor),
            (0.75, ResourcesColor),
            (1, ResourcesColor),
        };

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

        animatedExchangeRate.ControlUpdated += updateBackgroundGradient;
        updateBackgroundGradient(animatedExchangeRate.Value);

        var percentageLabel = TextFactories.Label(
            text: model.ExchangePercentage.Transform(r => $"{(int)(r * 100):0}%"),
            color: Binding.Constant(Color.Black)
        );
        percentageLabel.IsClickThrough = true;

        var rateLabel = TextFactories.Label(
            text: model.CoreEnergyToScrapRate.Transform(r => $"{r.Inverse.Value:0.0} > 1")
        );

        var background = new ComplexBox
        {
            Components = Window.BackgroundComponents,
            CornerRadius = Window.CornerRadius,
        };

        this.Add(
            [
                background,
                slider.Anchor(a => a.MarginAllSides(8).Top(relativePercentage: 0.5)),
                percentageLabel.Anchor(a => a.Top(relativePercentage: 0.5)),
                rateLabel.Anchor(a => a.Bottom(relativePercentage: 0.5)),
            ]
        );

        void updateBackgroundGradient(Vector2d p)
        {
            sliderBackgroundGradientStops[1] = (p.X, EnergyColor);
            sliderBackgroundGradientStops[2] = (p.X, ResourcesColor);
        }
    }
}
