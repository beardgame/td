using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.UI.Factories;
using Bearded.TD.UI.Reports;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

[ReportsOn(typeof(IStatisticsReport), ReportType.Effectivity)]
sealed class StatisticsReportControl : ReportControl
{
    public override double Height { get; }

    private readonly Binding<string> totalDamage = new();
    private readonly Binding<string> totalKills = new();

    private readonly Binding<string> currentWaveDamage = new();
    private readonly Binding<string> currentWaveKills = new();

    private readonly Binding<string> previousWaveDamage = new();
    private readonly Binding<string> previousWaveKills = new();

    private readonly IStatisticsReport report;

    public StatisticsReportControl(IStatisticsReport report)
    {
        this.report = report;

        var column = this.BuildFixedColumn();
        column
            .AddValueLabel("Total damage", totalDamage)
            .AddValueLabel("Total kills", totalKills)
            .AddValueLabel("Current wave damage", currentWaveDamage)
            .AddValueLabel("Current wave kills", currentWaveKills)
            .AddValueLabel("Previous wave damage", previousWaveDamage)
            .AddValueLabel("Previous wave kills", previousWaveKills);
        Height = column.Height;

        Update();
    }

    public override void Update()
    {
        totalDamage.SetFromSource($"{report.TotalDamage}");
        totalKills.SetFromSource($"{report.TotalKills}");
        currentWaveDamage.SetFromSource($"{report.CurrentWaveDamage}");
        currentWaveKills.SetFromSource($"{report.CurrentWaveKills}");
        previousWaveDamage.SetFromSource($"{report.PreviousWaveDamage}");
        previousWaveKills.SetFromSource($"{report.PreviousWaveKills}");
    }

    public override void Dispose() {}
}
