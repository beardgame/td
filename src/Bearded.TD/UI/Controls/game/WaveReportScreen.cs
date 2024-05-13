using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Tooltips;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.Content.CoreUI;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Game.Simulation.Statistics.WaveReport;

namespace Bearded.TD.UI.Controls;

sealed class WaveReportScreen : CompositeControl
{
    private const double width = 480;
    private const double rightMarginVisible = LayoutMargin;
    private static readonly double rightMarginHidden = -(LayoutMargin + width + Shadows.SmallWindow.PenumbraRadius);

    private static readonly AnimationFunction<(WaveReportScreen screen, float from, float to)> slide =
        AnimationFunction.ZeroToOne<(WaveReportScreen screen, float from, float to)>(0.3.S(),
            (s, t) => s.screen.setVisiblePercentage(Interpolate.Lerp(s.from, s.to, t)),
            s => s.screen.onSlideEnd()
        );

    private readonly GameInstance game;
    private readonly WaveReport report;
    private readonly Animations animations;
    private readonly TooltipFactory tooltips;

    private float visiblePercentage;
    private bool targetVisibility;
    private IAnimationController? currentSlideAnimation;
    private bool deleteAfterHiding;

    public WaveReportScreen(
        GameInstance game,
        WaveReport report,
        VoidEventHandler close,
        Animations animations,
        TooltipFactory tooltips)
    {
        this.game = game;
        this.report = report;
        this.animations = animations;
        this.tooltips = tooltips;

        this.Anchor(a => a.Right(rightMarginHidden, width));

        Add(WindowFactories.Window(b => b
            .WithTitle("Wave Report")
            .WithOnClose(close)
            .WithContent(buildWindowContent())
            .WithShadow(Shadows.SmallWindow)
        ));

        setVisiblePercentage(0);
        IsVisible = false;
    }

    private void setVisiblePercentage(float p)
    {
        IsVisible = true;
        visiblePercentage = p;
        var margin = Interpolation1d.SmoothStep.Interpolate(rightMarginHidden, rightMarginVisible, p);
        this.Anchor(a => a.Right(margin, width));
    }

    private void onSlideEnd()
    {
        IsVisible = targetVisibility;
        if (deleteAfterHiding && !IsVisible)
        {
            RemoveFromParent();
        }
    }

    public void Destroy()
    {
        if (IsVisible)
        {
            deleteAfterHiding = true;
            SetVisible(false);
        }
        else
        {
            RemoveFromParent();
        }
    }

    public void SetVisible(bool visible)
    {
        if (targetVisibility == visible)
            return;

        targetVisibility = visible;

        currentSlideAnimation?.Cancel();
        currentSlideAnimation = animations.Start(slide, (this, visiblePercentage, visible ? 1 : 0));
    }

    private Control buildWindowContent()
    {
        var content = new CompositeControl();
        var column = content.BuildScrollableColumn();

        addTopTowers(column);
        column.AddSeparator();
        addTotalDamageChart(column);

        return content;
    }

    private void addTopTowers(Layouts.IColumnLayout column)
    {
        const int towerCount = 6;
        const double towerWidth = width / towerCount - LayoutMarginSmall;
        const double towerHeight = towerWidth * 6 / 4;

        var rowContainer = new CompositeControl();
        var row = rowContainer.BuildFixedRow();

        var topTowers = report.AllTowers
            .OrderByDescending(t => t.TotalDamageDone.Amount.NumericValue)
            .Take(towerCount)
            .ToList();

        var mostEfficientTower = report.AllTowers.MaxBy(t => t.TotalEfficiency)?.GameObject;

        var margin = LayoutMarginSmall;
        var iconSize = towerWidth / 3;
        var smallIconSize = iconSize / 2;
        var smallIconMargin = (iconSize - smallIconSize) / 2;

        foreach (var tower in topTowers)
        {
            var model = TowerDamageDisplay.From(tower, game);
            var towerControl = new CompositeControl
            {
                ReportFactories.TowerDamageDisplay(model, animations, towerHeight, tooltips)
                    .Anchor(a => a.MarginAllSides(margin)),
            };
            row.AddLeft(towerControl, towerWidth);

            if (tower.GameObject == mostEfficientTower)
            {
                var wreath = new Sprite
                {
                    SpriteId = Sprites.HexWreath,
                    Color = Colors.DamageEfficiency(1),
                };

                towerControl.Add(
                    wreath.Anchor(a => a.Top(height: iconSize).Left(width: iconSize))
                );
            }
        }

        if (rowContainer.Children.FirstOrDefault() is CompositeControl mvp)
        {
            var star = new Sprite
            {
                SpriteId = Sprites.Star,
                Color = Colors.Experience,
            };
            var (m, s) = topTowers[0].GameObject == mostEfficientTower
                ? (smallIconMargin, smallIconSize)
                : (0, iconSize);
            mvp.Add(star.Anchor(a => a.Top(m, s).Left(m, s)));
        }

        var container = new CompositeControl
        {
            rowContainer.Anchor(a => a
                .Right(row.Width * -0.5f, relativePercentage: 0.5)
                .Left(row.Width * -0.5f, relativePercentage: 0.5)
                .Top(height: towerHeight)),
        };

        column.AddHeader("Top Towers", Colors.Get(ForeGroundColor.Headline2));
        column.Add(container, towerHeight + LayoutMargin);
    }

    private void addTotalDamageChart(Layouts.IColumnLayout column)
    {
        const double chartDiameter = width * 2 / 3;
        const double damageByTypeWidth = width - chartDiameter - LayoutMargin * 2;
        const double lineHeight = Text.LineHeight;
        const double height = chartDiameter + lineHeight + LayoutMargin;
        const double chartMargin = LayoutMargin;

        var container = makeContainer();

        var sortedDamage = report.AccumulatedDamageByType
            .OrderByDescending(d => d.DamageDone.Amount.NumericValue)
            .ToList();
        var totalDamage = AccumulatedDamage.Aggregate(sortedDamage.Select(d => d.AccumulatedDamage));

        var pieChart = ReportFactories.DamagePieChart(sortedDamage);
        var totalDamageLine = ReportFactories.SingleDamageAndEfficiency(totalDamage, lineHeight);
        var damageByType = ReportFactories.StackedDamageAndEfficiencies(sortedDamage);

        container.Add(
        [
            new CompositeControl { pieChart.Anchor(a => a.MarginAllSides(chartMargin)) }.Anchor(a => a
                .Left(width: chartDiameter)
                .Top(height: chartDiameter)
            ),
            totalDamageLine.Anchor(a => a
                .Left(margin: chartDiameter * 0.25, width: chartDiameter * 0.5)
                .Top(height: lineHeight, margin: chartDiameter + LayoutMargin)),
            damageByType.Anchor(a => a
                .Right(margin: LayoutMarginSmall, width: damageByTypeWidth)),
        ]);

        column.AddHeader("All Damage", Colors.Get(ForeGroundColor.Headline2));
        column.Add(container, height);
    }

    private static CompositeControl makeContainer() => [];
}
