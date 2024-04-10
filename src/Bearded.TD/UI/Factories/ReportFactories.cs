using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.UI.Tooltips;
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
    public static TowerDamageDisplay From(WaveReport.Tower data, GameInstance? game = null)
    {
        data.GameObject.TryGetSingleComponent<IObjectAttributes>(out var attributes);

        return new TowerDamageDisplay(
            attributes?.Name ?? "???",
            attributes?.Icon ?? Constants.Content.CoreUI.Sprites.QuestionMark,
            Binding.Constant(data.TotalDamageDone),
            Binding.Constant(data.TotalEfficiency),
            Binding.Constant(data.DamageByType),
            scrollTo(data.GameObject)
        );

        Action? scrollTo(GameObject obj) =>
            game switch
            {
                null => null,
                _ => () => game.CameraController.ScrollToWorldPos(obj.Position.XY()),
            };
    }
}

static class ReportFactories
{
    public static Control TowerDamageDisplay(
        TowerDamageDisplay model,
        Animations? animations = null,
        double? expectedControlHeight = null,
        TooltipFactory? tooltipFactory = null)
    {

        var expectedHeight = expectedControlHeight ?? 100;
        var expectedWidth = expectedHeight * (4f / 6f);

        var expectedContentHeight = expectedHeight - Constants.UI.LayoutMarginSmall * 2;
        var expectedContentWidth = expectedWidth - Constants.UI.LayoutMarginSmall * 2;

        var expectedContentRatio = expectedContentWidth / expectedContentHeight;

        var spriteSizeP = expectedContentRatio;
        var labelHeightP = (1 - expectedContentRatio) / 2;

        var labelHeight = expectedContentHeight * labelHeightP;
        var fontSize = labelHeight * 0.9f;

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

        var totalDamage = TextFactories.Label(
            text: model.TotalDamageDone.Transform(formatDamage),
            color: Binding.Constant(Constants.UI.Colors.Experience)
        );
        var totalEfficiency = TextFactories.Label(
            text: model.TotalEfficiency.Transform(formatEfficiency),
            color: model.TotalEfficiency.Transform(formatEfficiencyColor),
            textAnchor: (0.5, 0)
        );
        totalDamage.FontSize = fontSize;
        totalEfficiency.FontSize = fontSize;

        var content = new CompositeControl
        {
            sprite.Anchor(
                a => a.TopToBottomPercentage(0, spriteSizeP)),
            totalDamage.Anchor(
                a => a.TopToBottomPercentage(spriteSizeP, spriteSizeP + labelHeightP)),
            totalEfficiency.Anchor(
                a => a.TopToBottomPercentage(spriteSizeP + labelHeightP, spriteSizeP + labelHeightP * 2)
                    .Left(fontSize / 2)),
        };

        var backgroundShape = new ShapeComponent[1];
        var background = new ComplexBox
        {
            CornerRadius = expectedContentHeight / 14,
            Components = ShapeComponents.FromMutable(backgroundShape),
        };

        var button = new Button
        {
            background,
            content.Anchor(a => a.MarginAllSides(Constants.UI.LayoutMarginSmall)),
        };

        button.AnimateBackground(
            new ButtonBackgroundColor(
                Neutral: Constants.UI.Colors.Get(BackgroundColor.Element) * 0.1f,
                Hover: Constants.UI.Colors.Get(BackgroundColor.Hover) * 0.25f,
                Active: Constants.UI.Colors.Get(BackgroundColor.ActiveElement) * 0.5f
            ),
            color => backgroundShape[0] = Fill.With(color),
            () => backgroundShape[0],
            animations
        );

        if (model.OnClick is { } onClick)
            button.Clicked += _ => onClick();

        if (tooltipFactory != null)
        {
            var definition = createTooltipDefinition(model);
            button.Add(new TooltipTarget(tooltipFactory, definition, TooltipAnchor.Direction.Top));
        }

        return button;

    }

    private static Func<TooltipDefinition> createTooltipDefinition(TowerDamageDisplay model)
    {
        return () =>
        {
            var lineHeight = Constants.UI.Text.LineHeight;
            var lineCount = model.DamageByType.Value.Length + 1;

            return new TooltipDefinition(
                () => createControl(model, lineHeight),
                100,
                lineHeight * lineCount + Constants.UI.Tooltip.Margin * 2
            );
        };

        static Control createControl(TowerDamageDisplay model, float lineHeight)
        {
            var content = new CompositeControl();
            var column = content.BuildFixedColumn();

            column.AddLabel(model.Name, (0, 0.5));

            foreach (var damage in model.DamageByType.Value.OrderByDescending(d => d.DamageDone.Amount.NumericValue))
            {
                var row = new CompositeControl
                {
                    TextFactories.Label(formatDamage(damage.DamageDone), (1, 0.5), damage.Type.GetColor())
                        .Anchor(a => a.Right(relativePercentage: 0.45)),
                    TextFactories.Label(
                        formatEfficiency(damage.Efficiency),
                        (1, 0.5),
                        Constants.UI.Colors.DamageEfficiency(damage.Efficiency)
                    ),
                }.Anchor(a => a.Right(relativePercentage: 0.85));
                column.Add(row, lineHeight);
            }

            return TooltipFactories.TooltipWithContent(content);
        }
    }


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

    static string formatEfficiency(double efficiency) => $"{efficiency * 100:N0}%";
    static Color formatEfficiencyColor(double efficiency) => Constants.UI.Colors.DamageEfficiency(efficiency);
}
