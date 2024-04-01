using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class WaveReportScreen : CompositeControl
{
    public WaveReportScreen()
    {
        this.Anchor(a => a.Right(Constants.UI.LayoutMargin, 250));

        var window = WindowFactories.Window(b => b
            .WithTitle("Wave Report")
            .WithOnClose(() => { })
            .WithContent(buildContent())
            .WithShadow(Constants.UI.Shadows.SmallWindow)
        );

        Add(window);
    }

    private Control buildContent()
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

        column.AddHeader("Top Towers", Constants.UI.Colors.Get(ForeGroundColor.Headline2));
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

        column.AddHeader("All Damage", Constants.UI.Colors.Get(ForeGroundColor.Headline2));
        column.Add(container, height);
    }

    private static CompositeControl makeContainer()
    {
        return [];
    }
}
