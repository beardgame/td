using System;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Buildings.Ruins;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Debug;
using Bearded.TD.Game.Simulation.Exploration;
using Bearded.TD.Game.Simulation.Reports;
using Bearded.TD.Game.Simulation.Statistics;
using Bearded.TD.Game.Simulation.Upgrades;
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
            IRuinedReport ruinedReport => new RuinedReportControl(game, ruinedReport),
            IStatisticsReport statisticsReport => new StatisticsReportControl(statisticsReport),
            IUpgradeReport upgradeReport =>
                new UpgradeReportControl(upgradeReport.CreateInstance(game), detailsContainer),
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
