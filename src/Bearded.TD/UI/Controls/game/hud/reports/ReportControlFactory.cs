using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Buildings.Veterancy;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.UI.Factories;
using Bearded.TD.Utilities;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class ReportControlFactory : IReportControlFactory
{
    private readonly GameInstance game;
    private readonly IPulse pulse;

    public ReportControlFactory(GameInstance game, IPulse pulse)
    {
        this.game = game;
        this.pulse = pulse;
    }

    public Control CreateForReport(
        IReport report, Disposer disposer, ControlContainer detailsContainer, out double height)
    {
        var control = createForReport(report, detailsContainer);
        height = control.Height;
        pulse.Heartbeat += control.Update;
        disposer.AddDisposable(new ReportDisposer(control, pulse));
        return control;
    }

    private ReportControl createForReport(IReport report, ControlContainer detailsContainer)
    {
        return report switch
        {
            IBuildingStateReport buildingStateReport => new BuildingStateControl(game, buildingStateReport),
            IHealthReport healthReport => new HealthReportControl(healthReport),
            IManualControlReport manualControlReport => new ManualControlReportControl(game, manualControlReport),
            IStatisticsReport statisticsReport => new StatisticsReportControl(statisticsReport),
            ITargetingReport targetingReport => new TargetingReportControl(targetingReport),
            IUpgradeReport upgradeReport =>
                new UpgradeReportControl(upgradeReport.CreateInstance(game), detailsContainer),
            IVeterancyReport veterancyReport => new VeterancyReportControl(veterancyReport),
            IZoneRevealReport zoneRevealReport => new ZoneRevealReportControl(game, zoneRevealReport),
            IDebugReport debugReport => new DebugReportControl(debugReport),

            _ => throw new InvalidOperationException($"Cannot create control for report {report}")
        };
    }

    private sealed class ReportDisposer : IDisposable
    {
        private readonly ReportControl control;
        private readonly IPulse pulse;

        public ReportDisposer(ReportControl control, IPulse pulse)
        {
            this.control = control;
            this.pulse = pulse;
        }

        public void Dispose()
        {
            control.Dispose();
            pulse.Heartbeat -= control.Update;
        }
    }
}

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
