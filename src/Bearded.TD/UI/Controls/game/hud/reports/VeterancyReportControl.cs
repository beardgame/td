using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class VeterancyReportControl : ReportControl
{
    private readonly IVeterancyReport report;

    private readonly Binding<string> currentLevel = new();
    private readonly Binding<string> nextLevel = new();
    private readonly Binding<double> levelProgress = new();

    public override double Height { get; }

    public VeterancyReportControl(IVeterancyReport report)
    {
        this.report = report;
        var column = this.BuildFixedColumn();
        column
            .AddValueLabel("Current level", currentLevel)
            .AddValueLabel("Next level", nextLevel)
            .AddProgressBar(levelProgress);
        Height = column.Height;
        Update();
    }

    public override void Update()
    {
        currentLevel.SetFromSource(report.CurrentVeterancyLevel.ToString());
        nextLevel.SetFromSource(report.NextLevelThreshold == null
            ? "max!"
            : $"{report.CurrentExperience.ToUiString()} / {report.NextLevelThreshold.Value.ToUiString()}");
        levelProgress.SetFromSource(report.PercentageToNextLevel);
    }

    public override void Dispose() {}
}
