using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class WaveReportScreen : CompositeControl
{
    private const double width = 250;
    private const double rightMarginVisible = LayoutMargin;
    private static readonly double rightMarginHidden = -(LayoutMargin + width + Shadows.SmallWindow.PenumbraRadius);

    private static readonly AnimationFunction<(WaveReportScreen screen, float from, float to)> slide =
        AnimationFunction.ZeroToOne<(WaveReportScreen screen, float from, float to)>(0.3.S(),
            (s, t) => s.screen.setVisiblePercentage(Interpolate.Lerp(s.from, s.to, t)),
            s => s.screen.onSlideEnd()
        );

    private readonly Animations animations;

    private float visiblePercentage;
    private bool targetVisibility;
    private IAnimationController? currentSlideAnimation;
    private bool deleteAfterHiding;

    public WaveReportScreen(WaveReport report, VoidEventHandler close, Animations animations)
    {
        this.animations = animations;

        this.Anchor(a => a.Right(rightMarginHidden, width));

        Add(WindowFactories.Window(b => b
            .WithTitle("Wave Report")
            .WithOnClose(close)
            .WithContent(buildWindowContent(report))
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

    private Control buildWindowContent(WaveReport report)
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
        var container = makeContainer();
        // TODO: add towers to container

        column.AddHeader("Top Towers", Colors.Get(ForeGroundColor.Headline2));
        column.Add(container, 100);
    }

    private void addTotalDamageChart(Layouts.IColumnLayout column)
    {
        const double height = 200;
        const double chartDiameter = 200;

        var container = makeContainer();

        // TODO: add actual pie chart
        container.Add(new Dot().Anchor(a => a
            .Left(relativePercentage: 0.5, width: chartDiameter, margin: -chartDiameter / 2)
            .Top(relativePercentage: 0.5, height: chartDiameter, margin: -chartDiameter / 2)
        ));

        column.AddHeader("All Damage", Colors.Get(ForeGroundColor.Headline2));
        column.Add(container, height);
    }

    private static CompositeControl makeContainer() => [];
}
