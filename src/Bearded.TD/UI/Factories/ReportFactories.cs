using System;
using System.Collections.Immutable;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories;

sealed record TowerDamageDisplay(
    string Name,
    ModAwareSpriteId Sprite,
    IReadonlyBinding<UntypedDamage> TotalDamageDone,
    IReadonlyBinding<double> TotalEfficiency,
    IReadonlyBinding<ImmutableArray<WaveReport.TypedAccumulatedDamage>> DamageByType,
    Action? OnClick = null
)
{
    public static TowerDamageDisplay From(WaveReport.Tower data)
    {
        data.GameObject.TryGetSingleComponent<IObjectAttributes>(out var attributes);

        return new TowerDamageDisplay(
            attributes?.Name ?? "???",
            attributes?.Icon ?? Constants.Content.CoreUI.Sprites.QuestionMark,
            Binding.Constant(data.TotalDamageDone),
            Binding.Constant(data.TotalEfficiency),
            Binding.Constant(data.DamageByType),
            null //TODO: inject game to allow scrolling to tower
        );
    }
}

static class ReportFactories
{
    public static Control TowerDamageDisplay(TowerDamageDisplay model, Animations? animations = null)
    {
        const float spriteSize = 4f / 6f;
        const float labelHeight = 1f / 6f;

        var sprite = new Sprite
        {
            SpriteId = model.Sprite,
            Layout = new SpriteLayout(
                default,
                SpriteSize.ContainInFrame,
                frameAlign: (0.5f, 0),
                spriteAlign: (0.5f, 0),
                scale: (1, -1)
            ),
        };

        var totalDamage = TextFactories.Label(text: model.TotalDamageDone.Transform(formatDamage));
        var totalEfficiency = TextFactories.Label(
            text: model.TotalEfficiency.Transform(formatEfficiency),
            color: model.TotalEfficiency.Transform(formatEfficiencyColor)
        );

        var content = new CompositeControl
        {
            sprite.Anchor(
                a => a.TopToBottomPercentage(0, spriteSize)),
            totalDamage.Anchor(
                a => a.TopToBottomPercentage(spriteSize, spriteSize + labelHeight)),
            totalEfficiency.Anchor(
                a => a.TopToBottomPercentage(spriteSize + labelHeight, spriteSize + labelHeight * 2)),
        };

        var backgroundShape = new ShapeComponent[1];
        var background = new ComplexBox
        {
            CornerRadius = 6,
            Components = ShapeComponents.FromMutable(backgroundShape),
        };

        var button = new Button
        {
            background,
            content.Anchor(a => a.MarginAllSides(Constants.UI.LayoutMarginSmall)),
        };

        button.AnimateBackground(
            new ButtonBackgroundColor(
                Hover: Constants.UI.Colors.Get(BackgroundColor.Hover) * 0.25f,
                Active: Constants.UI.Colors.Get(BackgroundColor.ActiveElement) * 0.5f
            ),
            color => backgroundShape[0] = Fill.With(color),
            () => backgroundShape[0],
            animations
        );

        if (model.OnClick is { } onClick)
            button.Clicked += _ => onClick();

        return button;

        static string formatDamage(UntypedDamage damage)
        {
            var amount = damage.Amount.NumericValue;
            return amount switch
            {
                < 10000 => $"{amount:N0}",
                < 100_000 => $"{amount / 1000:N1}K",
                < 1_000_000 => $"{amount / 1000:N0}K",
                < 10_000_000 => $"{amount / 1000_000:N2}M",
                < 100_000_000 => $"{amount / 1000_000:N1}M",
                _ => $"{amount / 1000_000:N0}M",
            };
        }

        static string formatEfficiency(double efficiency) => $"{efficiency * 100:N0}";
        static Color formatEfficiencyColor(double efficiency) => Constants.UI.Colors.DamageEfficiency(efficiency);
    }
}
