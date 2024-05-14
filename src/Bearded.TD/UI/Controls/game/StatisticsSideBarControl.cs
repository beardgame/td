using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Animation;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;
using static Bearded.TD.Constants.UI.Button;

namespace Bearded.TD.UI.Controls;

sealed class StatisticsSideBarControl : CompositeControl
{
    private readonly StatisticsSideBar statistics;
    private readonly UIContext uiContext;

    private WaveReportScreen? currentWaveReport;

    public StatisticsSideBarControl(StatisticsSideBar statistics, UIContext uiContext)
    {
        this.statistics = statistics;
        this.uiContext = uiContext;
        IsClickThrough = true;

        Add(uiContext.Factories.Button(b => b
                .WithLabel("WR")
                .WithShadow()
                .MakeHexagon()
                .WithOnClick(statistics.OpenWaveReport)
                .WithEnabled(statistics.WaveReportButtonEnabled)
            )
            .BindIsVisible(statistics.WaveReportVisible.Transform(v => !v))
            .Anchor(a => a
                .Top(LayoutMargin * 2, SmallSquareButtonSize)
                .Right(LayoutMargin * 2, SmallSquareButtonSize))
        );

        statistics.LastWaveReport.SourceUpdated += onNewWaveReport;
        statistics.WaveReportVisible.SourceUpdated += v => currentWaveReport?.SetVisible(v);

        onNewWaveReport(statistics.LastWaveReport.Value);
    }

    private void onNewWaveReport(WaveReport? report)
    {
        currentWaveReport?.Destroy();
        currentWaveReport = null;

        if (report == null)
            return;

        currentWaveReport = new WaveReportScreen(statistics.Game, report, statistics.CloseWaveReport, uiContext);
        currentWaveReport.SetVisible(statistics.WaveReportVisible.Value);
        Add(currentWaveReport);
    }
}
