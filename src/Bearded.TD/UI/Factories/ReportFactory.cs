using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Statistics.Data;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.UI.Factories.TextFactories;
using Button = Bearded.UI.Controls.Button;

namespace Bearded.TD.UI.Factories;

sealed record TowerDamageDisplay(
    string Name,
    ModAwareSpriteId Sprite,
    IReadonlyBinding<UntypedDamage> TotalDamageDone,
    IReadonlyBinding<double> TotalEfficiency,
    IReadonlyBinding<ImmutableArray<TypedAccumulatedDamage>> DamageByType,
    Action? OnClick = null,
    ButtonHoverAction.HoverStartEffect<BuildingHighlighter.IHighlightedBuilding>? HighlightBuilding = null
)
{
    public static TowerDamageDisplay From(TowerStatistics data, GameInstance? game = null)
    {
        var attributes = data.Metadata.Attributes;

        return new TowerDamageDisplay(
            attributes.Name,
            attributes.Icon ?? Constants.Content.CoreUI.Sprites.QuestionMark,
            Binding.Constant(data.TotalDamageDone),
            Binding.Constant(data.TotalEfficiency),
            Binding.Constant(data.DamageByType),
            data.Metadata.LiveObject is { } o ? scrollTo(o) : null,
            data.Metadata.LiveObject is { } o1 ? highlight(o1) : null
        );

        Action? scrollTo(GameObject obj) =>
            game switch
            {
                null => null,
                _ => () => game.CameraController.ScrollToWorldPos(obj.Position.XY()),
            };

        ButtonHoverAction.HoverStartEffect<BuildingHighlighter.IHighlightedBuilding>? highlight(GameObject obj) =>
            game switch
            {
                null => null,
                _ => () => new BuildingHighlighter(game.Overlays).StartPersistentBuildingHighlight(obj)
            };
    }
}

sealed class ReportFactory(Animations animations, TooltipFactory tooltips)
{
    public Control TowerDamageDisplay(TowerDamageDisplay model, double? expectedControlHeight = null)
    {
        var expectedHeight = expectedControlHeight ?? 100;
        var expectedWidth = expectedHeight * (4f / 6f);

        var expectedContentHeight = expectedHeight - LayoutMarginSmall * 2;
        var expectedContentWidth = expectedWidth - LayoutMarginSmall * 2;

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

        var totalDamage = Label(
            text: model.TotalDamageDone.Transform(formatDamage),
            color: Binding.Constant(Colors.Experience)
        );
        var totalEfficiency = Label(
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
            content.Anchor(a => a.MarginAllSides(LayoutMarginSmall)),
        };

        button.AnimateBackground(
            new ButtonBackgroundColor(
                Neutral: Colors.Get(BackgroundColor.Element) * 0.1f,
                Hover: Colors.Get(BackgroundColor.Hover) * 0.25f,
                Active: Colors.Get(BackgroundColor.ActiveElement) * 0.5f
            ),
            color => backgroundShape[0] = Fill.With(color),
            () => backgroundShape[0],
            animations
        );

        if (model.OnClick is { } onClick)
            button.Clicked += _ => onClick();
        if (model.HighlightBuilding is { } highlightBuilding)
            button.AddHoverAction(highlightBuilding, b => b.EndHighlight());

        // TODO: button builders should just support tooltips
        var definition = createTooltipDefinition(model);
        button.Add(new TooltipTarget(tooltips, definition, TooltipAnchor.Direction.Top));

        return button;
    }

    private static Func<TooltipDefinition> createTooltipDefinition(TowerDamageDisplay model)
    {
        return () =>
        {
            var lineHeight = Text.LineHeight;
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

            var sortedDamage = model.DamageByType.Value.OrderByDescending(d => d.DamageDone.Amount.NumericValue);
            AddStackedDamageAndEfficiencies(column, sortedDamage, lineHeight);

            return TooltipFactories.TooltipWithContent(
                new CompositeControl { content.Anchor(a => a.Right(relativePercentage: 0.85)) }
            );
        }
    }

    public static Control DamagePieChart(IEnumerable<TypedAccumulatedDamage> data)
    {
        const float shadow = 0.05f;

        var pieChart = PieChartFactories.StaticPieChart(data, b => b
            .WithValues(d => d.DamageDone.Amount.NumericValue)
            .WithColors(d => d.Type.GetColor())
            .WithAdditionalComponents([
                Fill.With(ShapeColor.From(
                    [
                        (0, Color.White),
                        (1 - shadow - 0.01f, Color.White),
                        (1 - shadow, Color.Gray),
                        (1, Color.Gray),
                    ],
                    GradientDefinition.Radial(
                        AnchorPoint.Relative((0.5f, 0.5f - shadow * 0.5f)),
                        AnchorPoint.Relative((0.5f, 1))
                    ).WithBlendMode(ComponentBlendMode.Multiply)
                )),
                Fill.With(ShapeColor.From(
                    [
                        (0, Color.White),
                        (1, new Color(0xFF666666)),
                    ],
                    GradientDefinition.Linear(
                        AnchorPoint.Relative((0, 0)),
                        AnchorPoint.Relative((0.5f, 1))
                    ).WithBlendMode(ComponentBlendMode.Multiply)
                )),
            ])
        );

        return pieChart;
    }

    public static Control StackedDamageAndEfficiencies(
        IEnumerable<TypedAccumulatedDamage> damage,
        double lineHeight = Text.LineHeight,
        double? fontSizeOverride = null)
    {
        var content = new CompositeControl();
        var column = content.BuildFixedColumn();
        AddStackedDamageAndEfficiencies(column, damage, lineHeight, fontSizeOverride);
        return content;
    }

    public static void AddStackedDamageAndEfficiencies(
        Layouts.IColumnLayout column,
        IEnumerable<TypedAccumulatedDamage> damage,
        double lineHeight = Text.LineHeight,
        double? fontSizeOverride = null)
    {
        var fontSize = fontSizeOverride ?? lineHeight * (Text.FontSize / Text.LineHeight);
        foreach (var d in damage)
        {
            var row = SingleDamageAndEfficiency(d, fontSize);
            column.Add(row, lineHeight);
        }
    }

    public static Control SingleDamageAndEfficiency(
        AccumulatedDamage damage, double fontSize = Text.FontSize)
    {
        var damageColor = Colors.Experience;
        var damageDone = damage.DamageDone;
        var damageEfficiency = damage.Efficiency;

        return singleDamageAndEfficiency(fontSize, damageEfficiency, damageDone, damageColor);
    }

    public static Control SingleDamageAndEfficiency(
        TypedAccumulatedDamage damage, double fontSize = Text.FontSize)
    {
        var damageColor = damage.Type.GetColor();
        var damageDone = damage.DamageDone;
        var damageEfficiency = damage.Efficiency;

        return singleDamageAndEfficiency(fontSize, damageEfficiency, damageDone, damageColor);
    }

    private static Control singleDamageAndEfficiency(
        double fontSize, double damageEfficiency, UntypedDamage damageDone, Color damageColor)
    {
        var anchor = (1, 0.5);
        var efficiencyColor = Colors.DamageEfficiency(damageEfficiency);

        var damageLabel = Label(formatDamage(damageDone), anchor, damageColor);
        var efficiencyLabel = Label(formatEfficiency(damageEfficiency), anchor, efficiencyColor);

        damageLabel.FontSize = fontSize;
        efficiencyLabel.FontSize = fontSize;

        return new CompositeControl
        {
            damageLabel.Anchor(a => a.Right(relativePercentage: 0.5)),
            efficiencyLabel.Anchor(a => a.Left(relativePercentage: 0.5)),
        };
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
    static Color formatEfficiencyColor(double efficiency) => Colors.DamageEfficiency(efficiency);
}
