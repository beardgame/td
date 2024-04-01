using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.UI.Controls;
using Bearded.Utilities;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Controls;

sealed class WaveReportScreen : CompositeControl
{
    private readonly Animations animations;

    public WaveReportScreen(WaveReport report, VoidEventHandler close, Animations animations)
    {
        this.animations = animations;

        this.Anchor(a => a.Right(LayoutMargin, 250));

        Add(WindowFactories.Window(b => b
            .WithTitle("Wave Report")
            .WithOnClose(close)
            .WithContent(buildWindowContent(report))
            .WithShadow(Shadows.SmallWindow)
        ));

        IsVisible = false;
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

    private static CompositeControl makeContainer()
    {
        return [];
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }
}
